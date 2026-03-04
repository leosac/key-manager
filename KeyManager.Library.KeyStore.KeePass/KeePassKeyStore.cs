using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Serialization;
using Leosac.KeyManager.Library.KeyStore.CNG;
using Leosac.KeyManager.Library.KeyStore.HSM_PKCS11;
using Leosac.KeyManager.Library.KeyStore.LCP;
using Leosac.KeyManager.Library.KeyStore.Memory;
using Leosac.KeyManager.Library.KeyStore.NXP_SAM;
using Leosac.KeyManager.Library.KeyStore.SAM_SE;
using Leosac.KeyManager.Library.Plugin;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Vanara.Extensions.Reflection;

namespace Leosac.KeyManager.Library.KeyStore.KeePass
{
    public class KeePassKeyStore : KeyStore
    {
        private static readonly TimeSpan OPEN_TIMEOUT = TimeSpan.FromSeconds(30);
        private const string CUSTOM_KEYDATA_FIELD = "KeyData_Leosac";
        private const string VARIANT_NAME_FIELD = "KeyVariantName_Leosac";
        private const string KEY_VERSION_FIELD = "KeyVersion_Leosac";
        private const string DATE_MODIFICATION_FIELD = "LastModification_Leosac"; //Last modification from LKM

        public override string Name => "KeePass";
        private PwDatabase? _database;
        private CompositeKey? _masterKey;

        private PwGroup? _folder;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(KeePassKeyStore));

        public override bool CanCreateKeyEntries => true;
        public override bool CanDeleteKeyEntries => true;
        public override bool CanUpdateKeyEntries => true;
        public bool IgnoreIncompleteEntries { get; set; } = false;
        public override IEnumerable<KeyEntryClass> SupportedClasses => [KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric];

        new event EventHandler<IChangeKeyEntry>? KeyEntryUpdated;

        private static readonly JsonSerializerSettings SerializerSettings = KeyEntry.CreateJsonSerializerSettings();

        private static readonly Dictionary<Type, string> StoreCache = [];
        private static readonly JsonSerializer CachedSerializer = JsonSerializer.Create(SerializerSettings);


        public KeePassKeyStore()
        {
            Properties = new KeePassKeyStoreProperties();
        }


        public override async Task Open()
        {
            var fileProps = GetFileProperties();
            try
            {
                CheckPreconditions();
                Cleanup();
                _database = new PwDatabase();
                _masterKey = BuildMasterKey();
                var ioc = new IOConnectionInfo { Path = fileProps.DBPath };
                ValidateIOConnection(ioc);
                using var cts = new CancellationTokenSource(OPEN_TIMEOUT);
                await Task.Run(() =>
                {
                    cts.Token.ThrowIfCancellationRequested();
                    _database.Open(ioc, _masterKey, null);
                }, cts.Token).ConfigureAwait(false);
                CheckState();
                _folder = string.IsNullOrEmpty(fileProps.ProfilePath)
                    ? _database.RootGroup
                    : FindGroupByName(fileProps.ProfilePath) ?? _database.RootGroup;
                log.Info($"KeePass .kdbx file opened successfully : {fileProps.DBPath}");
            }
            catch (OperationCanceledException)
            {
                log.Error($"KeePass database open timeout after {OPEN_TIMEOUT.TotalSeconds}s");
                Cleanup();
                throw new KeyStoreException($"KeePass database open timeout after {OPEN_TIMEOUT.TotalSeconds}s");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to open KeePass .kdbx file: {ex.Message}", ex);
                Cleanup();
                throw new KeyStoreException("Failed to open KeePass database file.", ex);
            }
        }

        private PwGroup? FindGroupByName(string name)
        {
            var root = _database?.RootGroup;
            if (root == null || string.IsNullOrEmpty(name))
                return root;
            var comparer = StringComparer.OrdinalIgnoreCase;
            var stack = new Stack<PwGroup>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var g = stack.Pop();
                if (!string.IsNullOrEmpty(g?.Name) && comparer.Equals(g.Name.Trim(), name.Trim()))
                    return g;

                if (g?.Groups != null)
                    foreach (var child in g.Groups)
                        if (child != null) stack.Push(child);
            }
            return root; // fallback if not found
        }

        public override async Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            ArgumentNullException.ThrowIfNull(identifier);
            if (string.IsNullOrWhiteSpace(identifier.Id)) return false;

            await CheckOpen().ConfigureAwait(false);

            return _folder?.Entries?.Any(e =>
                MatchesEntryTitle(e, identifier.Id) &&
                (!IgnoreIncompleteEntries || !string.IsNullOrEmpty(GetSafeString(e, CUSTOM_KEYDATA_FIELD)))
            ) == true;
        }

        public override async Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass)
        {
            await CheckOpen().ConfigureAwait(false);
            log.Info(string.Format("Getting all key entries (class : `{0}`)...", keClass));
            var keyEntries = new List<KeyEntryId>();
            if (_folder?.Entries == null) return [];
            foreach (var entry in _folder.Entries)
            {
                if (IgnoreIncompleteEntries && string.IsNullOrEmpty(GetSafeString(entry, CUSTOM_KEYDATA_FIELD)))
                    continue;
                var id = entry.Strings.ReadSafe(PwDefs.TitleField);
                if (string.IsNullOrWhiteSpace(id)) continue;
                var keyEntryId = new KeyEntryId { Id = id };
                if (GetLabel(entry, out var label) && !string.IsNullOrWhiteSpace(label))
                    keyEntryId.Label = label;
                keyEntries.Add(keyEntryId);
            }
            log.Info($"{keyEntries.Count} key entries returned.");
            return keyEntries;
        }

        public override async Task Create(IChangeKeyEntry keyEntry)
        {
            ArgumentNullException.ThrowIfNull(keyEntry);
            ValidateIdentifier(keyEntry.Identifier);
            if (await CheckKeyEntryExists(keyEntry.Identifier, keyEntry.KClass).ConfigureAwait(false))
            {
                log.Error(string.Format($"A key entry with the same identifier `{keyEntry.Identifier}` already exists."));
                throw new KeyStoreException("A key entry with the same identifier already exists.");
            }
            await CheckOpen().ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(_folder, nameof(_folder));
            ArgumentNullException.ThrowIfNull(_database, nameof(_database));
            var entry = CreateKeePassEntry(keyEntry);
            _folder.AddEntry(entry, true);
            SaveDatabase();
            log.Info($"Created key entry : {keyEntry.Identifier.Id}");
        }

        public override async Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            if (identifier == null || string.IsNullOrWhiteSpace(identifier.Id))
                throw new ArgumentException("Identifier required", nameof(identifier));
            await CheckOpen().ConfigureAwait(false);
            var entry = FindEntryByTitle(identifier.Id) ?? throw new KeyStoreException($"Key entry '{identifier}' doesn't exist.");
            var metadataJson = GetSafeString(entry, CUSTOM_KEYDATA_FIELD);
            if (string.IsNullOrEmpty(metadataJson))
            {
                log.Warn($"No metadata found for entry : {identifier.Id}");
                return null;
            }
            try
            {
                var obj = JObject.Parse(metadataJson);
                var _kclass = obj["KClass"]?.ToString();
                if (!Enum.TryParse<KeyEntryClass>(_kclass, out var kclass) || kclass != keClass)
                {
                    log.Warn($"KClass mismatch for {identifier.Id}: expected {keClass}, got {_kclass}");
                    return null;
                }
                obj["Variant"] = BuildVariant(entry);
                var keyEntry = CreateFromJson(obj, kclass, identifier);
                if (keyEntry == null)
                {
                    log.Warn($"Failed to create KeyEntry for {identifier.Id}");
                    return null;
                }
                PopulateKeyEntry(keyEntry, obj);
                OnKeyEntryRetrieved(keyEntry);
                log.Info($"Key entry `{identifier}` retrieved successfully.");
                return keyEntry;
            }
            catch (JsonException ex)
            {
                log.Warn($"JSON deserialization failed for {identifier.Id}: {ex.Message}", ex);
                return null;
            }
            catch (Exception ex)
            {
                log.Warn($"Failure when deserializing key entry {identifier.Id}: {ex.Message}", ex);
                return null;
            }
        }

        private static JObject BuildVariant(PwEntry entry)
        {
            var vName = GetSafeString(entry, VARIANT_NAME_FIELD) ?? "KeyVariant";
            var versionFields = entry.Strings.GetKeys()
                .Where(k => k.StartsWith(KEY_VERSION_FIELD, StringComparison.OrdinalIgnoreCase))
                .OrderBy(k => k).ToList();
            var containers = new JArray();
            var failedFields = new List<string>();
            foreach (var field in versionFields)
            {
                try
                {
                    var versionJson = GetSafeString(entry, field);
                    if (string.IsNullOrEmpty(versionJson)) continue;
                    var versionObj = JObject.Parse(versionJson);
                    var name = versionObj[nameof(KeyContainer.Name)]?.ToString() ?? nameof(KeyContainer.Name);
                    var keyObj = new JObject
                    {
                        ["KeySize"] = versionObj["KeySize"] ?? (JToken)0u,
                        ["Tags"] = versionObj["Tags"] ?? new JArray(),
                        ["Materials"] = new JArray(),
                        ["Link"] = new JObject
                        {
                            ["KeyIdentifier"] = new JObject { ["Id"] = "" },
                            ["DivInput"] = new JArray()
                        }
                    };
                    if (versionObj[nameof(Key.Materials)] is JArray mats && keyObj[nameof(Key.Materials)] is JArray materialsArray)
                    {
                        foreach (var m in mats)
                        {
                            if (m is not JObject materialObj) continue;
                            var value = materialObj[nameof(KeyMaterial.Value)]?.ToString();
                            if (string.IsNullOrEmpty(value)) continue;
                            materialsArray.Add(new JObject
                            {
                                [nameof(KeyMaterial.Value)] = value,
                                [nameof(KeyMaterial.OverrideSize)] = materialObj[nameof(KeyMaterial.OverrideSize)] ?? (JToken)0u,
                                [nameof(KeyMaterial.IsRealKeyMateriel)] = true
                            });
                        }
                    }
                    var containerObj = new JObject
                    {
                        ["$type"] = "Leosac.KeyManager.Library.KeyStore.KeyVersion, KeyManager.Library",
                        [nameof(KeyVersion.Version)] = keyObj["Version"] ?? 0,
                        [nameof(KeyVersion.Key)] = keyObj,
                        [nameof(KeyVersion.Name)] = name
                    };
                    containers.Add(containerObj);
                }
                catch (Exception ex)
                {
                    failedFields.Add(field);
                    log.Warn($"Failed to parse {field}: {ex.Message}", ex);
                }
            }
            if (failedFields.Count > 0)
                log.Warn($"Variant skipped {failedFields.Count} malformed field(s): {string.Join(", ", failedFields)}");
            return new JObject
            {
                [nameof(KeyEntryVariant.Name)] = vName,
                [nameof(KeyEntryVariant.KeyContainers)] = containers
            };
        }

        private static KeyEntry? CreateFromJson(JObject obj, KeyEntryClass kclass, KeyEntryId identifier)
        {
            if (obj["Properties"]?["$type"] is JToken typeToken)
            {
                var keyEntryType = MapToKeyEntry(typeToken.ToString());
                if (keyEntryType != null)
                {
                    var keyEntry = (KeyEntry?)Activator.CreateInstance(keyEntryType);
                    keyEntry!.Identifier = identifier;
                    return keyEntry;
                }
            }
            return kclass switch //Fallback
            {
                KeyEntryClass.Symmetric => new SAMSymmetricKeyEntry { Identifier = identifier },
                KeyEntryClass.Asymmetric => new AsymmetricPKCS11KeyEntry { Identifier = identifier },
                _ => null
            };
        }

        private static Type? MapToKeyEntry(string prop) =>
            prop switch
            {
                var t when t.Contains("CNGKeyEntryProperties") => typeof(CNGKeyEntry),
                var t when t.Contains("SymmetricPKCS11KeyEntryProperties") => typeof(SymmetricPKCS11KeyEntry),
                var t when t.Contains("LCPKeyEntryProperties") => typeof(LCPKeyEntry),
                var t when t.Contains("MemoryKeyEntryProperties") => typeof(MemoryKeyEntry),
                var t when t.Contains("SAMSymmetricKeyEntryProperties") => typeof(SAMSymmetricKeyEntry),
                var t when t.Contains("SAM_SESymmetricKeyEntryAuthenticationProperties") ||
                        t.Contains("SAM_SESymmetricKeyEntryDESFireUIDProperties") ||
                        t.Contains("SAM_SESymmetricKeyEntryDESFireProperties") => typeof(SAM_SESymmetricKeyEntry),
                var t when t.Contains("SAMAsymmetricKeyEntryProperties") => typeof(AsymmetricPKCS11KeyEntry),
                var t when t.Contains("AsymmetricPKCS11KeyEntryProperties") => typeof(AsymmetricPKCS11KeyEntry),
                _ => null
            };

        private static bool GetLabel(PwEntry entry, out string? label)
        {
            var labelString = entry.Strings.GetSafe(PwDefs.UserNameField);
            label = labelString?.ReadString();
            return !string.IsNullOrWhiteSpace(label);
        }

        private static void PopulateKeyEntry(KeyEntry keyEntry, JObject obj)
        {
            SetProperties(keyEntry, obj["Properties"] as JObject);
            SetVariant(keyEntry, obj["Variant"] as JObject);
        }

        private static void SetProperties(KeyEntry keyEntry, JObject? obj)
        {
            if (obj == null) return;
            try
            {
                var prop = JsonConvert.DeserializeObject<KeyEntryProperties>(obj.ToString(Formatting.None), SerializerSettings);
                if (prop == null)
                    return;
                var propFactory = KeyEntryFactory.GetFactoryFromPropertyType(prop.GetType());
                keyEntry.Properties = propFactory?.CreateKeyEntryProperties(obj.ToString(Formatting.None)) ?? prop;
            }
            catch (JsonException ex)
            {
                log.Warn($"Failed to deserialize key entry properties : {ex.Message}", ex);
            }
        }


        private static void SetVariant(KeyEntry keyEntry, JObject? variant)
        {
            if (variant == null) return;
            keyEntry.Variant ??= new KeyEntryVariant();
            JsonConvert.PopulateObject(variant.ToString(Formatting.None), keyEntry.Variant, SerializerSettings);
            SetContainers(keyEntry.Variant, variant["KeyContainers"] as JArray);
        }

        private static void SetContainers(KeyEntryVariant variant, JArray? containers)
        {
            if (containers?.HasValues != true) return;
            variant.KeyContainers.Clear();
            foreach (var c in containers.Cast<JObject>())
            {
                try
                {
                    var kc = c.ToObject<KeyContainer>(CachedSerializer);
                    if (kc != null)
                        variant.KeyContainers.Add(kc);
                }
                catch (JsonException ex)
                {
                    log.Warn($"Failed to populate KeyContainer : {ex.Message}", ex);
                }
            }
        }

        private static string GetSafeString(PwEntry entry, string field) =>
            entry.Strings.GetSafe(field) is ProtectedString ps ? ps.ReadString() : string.Empty;

        private static bool MatchesEntryTitle(PwEntry entry, string title) =>
            string.Equals(entry.Strings.ReadSafe(PwDefs.TitleField), title, StringComparison.OrdinalIgnoreCase);

        public override async Task Update(IChangeKeyEntry change, bool ignoreIfMissing)
        {
            ArgumentNullException.ThrowIfNull(change);
            ValidateIdentifier(change.Identifier);
            await CheckOpen().ConfigureAwait(false);
            var existing = FindEntryByTitle(change.Identifier.Id!);
            if (existing == null)
            {
                if (ignoreIfMissing) return;
                throw new KeyNotFoundException($"Key entry not found : {change.Identifier}");
            }
            UpdateKeePassEntry(existing, change);
            SaveDatabase();
            OnKeyEntryUpdated(change);
            log.Info($"Key entry `{change.Identifier}` updated.");
        }

        private static PwEntry CreateKeePassEntry(IChangeKeyEntry keyEntry)
        {
            ArgumentNullException.ThrowIfNull(keyEntry);
            if (keyEntry is not KeyEntry keyEntryInstance)
                throw new ArgumentException("KeyEntry instance not supported", nameof(keyEntry));
            var entry = new PwEntry(true, true) { Uuid = new PwUuid(true) };
            UpdateKeePassEntry(entry, keyEntryInstance);
            return entry;
        }

        private static void UpdateKeePassEntry(PwEntry entry, IChangeKeyEntry keyEntry)
        {
            ArgumentNullException.ThrowIfNull(keyEntry);
            if (keyEntry is not KeyEntry keyEntryInstance)
                throw new ArgumentException("KeyEntry instance not supported.", nameof(keyEntry));
            try
            {
                var metadataObj = new
                {
                    keyEntryInstance.KClass,
                    keyEntryInstance.Properties
                };
                var metadataJson = JsonConvert.SerializeObject(metadataObj, SerializerSettings);
                var metadata = JObject.Parse(metadataJson);
                entry.Strings.Set(CUSTOM_KEYDATA_FIELD, new ProtectedString(true, metadata.ToString(Formatting.Indented)));
                if (!string.IsNullOrWhiteSpace(keyEntryInstance.Variant?.Name))
                    entry.Strings.Set(VARIANT_NAME_FIELD, new ProtectedString(false, keyEntryInstance.Variant.Name));
                ResetKeyVersionFields(entry);
                StoreKeyVersions(entry, keyEntryInstance.Variant?.KeyContainers);
                SetKeePassFields(entry, keyEntryInstance);
            }
            catch (Exception ex)
            {
                log.Error($"Failed to update KeePass entry : {ex.Message}", ex);
                throw new KeyStoreException("Failed to serialize key entry data", ex);
            }
        }

        private static void SetKeePassFields(PwEntry entry, KeyEntry keyEntry)
        {
            var firstKey = ExtractKey(keyEntry.Variant);
            if (!string.IsNullOrEmpty(firstKey))
                entry.Strings.Set(PwDefs.PasswordField, new ProtectedString(true, firstKey));
            entry.Strings.Set(PwDefs.TitleField, new ProtectedString(true, keyEntry.Identifier.Id!));
            if (!string.IsNullOrWhiteSpace(keyEntry.Identifier.Label))
                entry.Strings.Set(PwDefs.UserNameField, new ProtectedString(true, keyEntry.Identifier.Label));
            var notes = BuildNotes(keyEntry);
            if (!string.IsNullOrEmpty(notes))
                entry.Strings.Set(PwDefs.NotesField, new ProtectedString(false, notes));
            entry.Strings.Set(PwDefs.UrlField, new ProtectedString(false, $"key://{keyEntry.Identifier.Id}"));
            SetEntryIcon(entry, keyEntry);
            entry.Strings.Set(DATE_MODIFICATION_FIELD, new ProtectedString(false, DateTime.UtcNow.ToString("G")));
        }

        private static string? ExtractKey(KeyEntryVariant? variant)
        {
            if (variant?.KeyContainers?.Any() != true)
                return null;
            return variant.KeyContainers
                .SelectMany(c => c.Key?.Materials ?? [])
                .FirstOrDefault(m => m.IsRealKeyMateriel && !string.IsNullOrEmpty(m.Value))
                ?.Value;
        }

        private static void ResetKeyVersionFields(PwEntry entry)
        {
            foreach (var key in entry.Strings.GetKeys()
                .Where(k => k.StartsWith(KEY_VERSION_FIELD, StringComparison.OrdinalIgnoreCase)).ToList())
            {
                entry.Strings.Remove(key);
            }
        }

        private static void StoreKeyVersions(PwEntry entry, IEnumerable<KeyContainer>? containers)
        {
            if (containers?.Any() != true)
                return;
            int index = 0;
            foreach (var container in containers.Where(c => c?.Key != null))
            {
                var fieldName = $"{KEY_VERSION_FIELD}_{SetFieldName(container.Name ?? $"V{index}")}";
                var _jKey = JsonConvert.SerializeObject(KeyVersionSummary(container), Formatting.Indented);
                entry.Strings.Set(fieldName, new ProtectedString(true, _jKey));
                index++;
            }
        }

        private static object KeyVersionSummary(KeyContainer container)
        {
            var key = container.Key;
            var tags = key.Tags?.Where(t => !string.IsNullOrWhiteSpace(t)).ToList() ?? [];
            var keyMaterialsList = key.Materials?
                .Where(m => m.IsRealKeyMateriel && !string.IsNullOrEmpty(m.Value))
                .Select(m => new JObject
                {
                    ["Value"] = m.Value,
                    ["OverrideSize"] = m.OverrideSize
                }).Cast<JToken>().ToList() ?? [];
            return new
            {
                Name = container.Name ?? "Undefined",
                key.KeySize,
                Tags = tags,
                Materials = new JArray(keyMaterialsList),
                IsConfigured = container.IsConfigured()
            };
        }

        private static string BuildNotes(KeyEntry keyEntry)
        {
            var variant = keyEntry.Variant;
            if (variant?.KeyContainers == null)
                return $"Key Type : Unknown{Environment.NewLine}Class : {keyEntry.KClass}{Environment.NewLine}Store : {ExtractStoreType(keyEntry.Properties)}";
            var storeType = ExtractStoreType(keyEntry.Properties);
            var sb = new StringBuilder(512)
                .Append($"Key Type : {variant.Name ?? "Unknown"}")
                .Append(Environment.NewLine)
                .Append($"Class : {keyEntry.KClass}")
                .Append(Environment.NewLine)
                .Append($"Size : {(variant.KeyContainers[0].Key.KeySize * 8)} bits")
                .Append(Environment.NewLine);
            var firstTags = variant.KeyContainers[0].Key.Tags;
            if (firstTags?.Count > 0)
                sb.Append($"Tags : {string.Join(", ", firstTags.Take(5))}").Append(Environment.NewLine);
            sb.Append($"Store : {storeType}").Append(Environment.NewLine);
            var containerCount = variant.KeyContainers.Count;
            if (containerCount > 1)
                sb.Append($"Versions : {containerCount}").Append(Environment.NewLine);
            for (int i = 0; i < Math.Min(3, containerCount); i++)
            {
                var container = variant.KeyContainers[i];
                var name = container.Name ?? $"V{i + 1}";
                var size = container.Key.KeySize * 8;
                var tagsPreview = string.Join("/", container.Key.Tags?.Take(2) ?? []);
                sb.Append($"  ↳ {name}: {size}b ({tagsPreview})").Append(Environment.NewLine); //UTF-8 symbol for pretty output
            }
            return sb.ToString().TrimEnd();
        }

        private static string ExtractStoreType(object? prop)
        {
            if (prop == null)
                return "Unknown";
            if (!StoreCache.TryGetValue(prop.GetType(), out var _type))
            {
                try
                {
                    var typeStr = prop.ToString()?.Split(',')[0] ?? "";
                    _type = typeStr.Split('.').Last()
                        ?.Replace("Properties", "", StringComparison.OrdinalIgnoreCase)
                        ?? "Unknown";
                    StoreCache[prop.GetType()] = _type;
                }
                catch
                {
                    _type = "Unknown";
                }
            }
            return _type;
        }

        private static void SetEntryIcon(PwEntry entry, KeyEntry keyEntry)
        {
            entry.IconId = keyEntry.KClass switch
            {
                KeyEntryClass.Symmetric => PwIcon.Key,
                KeyEntryClass.Asymmetric => PwIcon.Certificate,
                _ => PwIcon.Console
            };
        }

        public override async Task Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing)
        {
            ArgumentNullException.ThrowIfNull(identifier);
            if (string.IsNullOrEmpty(identifier.Id))
                return;
            await CheckOpen().ConfigureAwait(false);
            var entry = FindEntryByTitle(identifier.Id!);
            if (entry == null)
            {
                if (ignoreIfMissing)
                    return;
                throw new KeyNotFoundException($"Key entry '{identifier.Id}' not found");
            }
            if (_folder == null || _database == null)
                throw new InvalidOperationException("Database or profile not initialized.");
            var del = _folder!.Entries.Remove(entry);
            if (del)
            {
                _database.DeletedObjects.Add(new PwDeletedObject(entry.Uuid, DateTime.Now));
                SaveDatabase();
                log.Info($"Deleted key entry : {identifier.Id}");
            }
            else
            {
                log.Warn($"Failed to remove entry from profile : {identifier.Id}");
            }
        }

        private CompositeKey BuildMasterKey()
        {
            var key = new CompositeKey();
            var props = GetFileProperties();
            if (!string.IsNullOrEmpty(props.Secret))
                key.AddUserKey(new KcpPassword(props.Secret));
            if (!string.IsNullOrEmpty(props.KeyPath) && File.Exists(props.KeyPath))
                key.AddUserKey(new KcpKeyFile(props.KeyPath));
            if (key.UserKeyCount == 0)
                throw new InvalidOperationException("At least one credential (password, key file, or both) required");
            return key;
        }

        private PwEntry? FindEntryByTitle(string title) =>
            string.IsNullOrWhiteSpace(title) ? null : _folder?.Entries?.FirstOrDefault(e => MatchesEntryTitle(e, title));

        private static void ValidateIOConnection(IOConnectionInfo ioc)
        {
            if (string.IsNullOrWhiteSpace(ioc.Path))
                throw new ArgumentException("Invalid IO connection path", nameof(ioc));
        }

        private void CheckState()
        {
            ArgumentNullException.ThrowIfNull(_database);
            if (!_database.IsOpen)
                throw new InvalidOperationException("Database not opened");
        }

        protected new void OnKeyEntryUpdated(IChangeKeyEntry keyEntry)
        {
            KeyEntryUpdated?.Invoke(this, keyEntry);
            base.OnKeyEntryUpdated(keyEntry);
        }

        private async Task CheckOpen()
        {
            if (_database?.IsOpen != true)
                await Open().ConfigureAwait(false);
        }

        private static void ValidateIdentifier(KeyEntryId identifier)
        {
            ArgumentNullException.ThrowIfNull(identifier);
            if (string.IsNullOrWhiteSpace(identifier.Id))
                throw new ArgumentException("KeyEntry ID required", nameof(identifier));
        }

        private void SaveDatabase()
        {
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            _database.Modified = true;
            _database.Save(null);
        }

        private void Cleanup()
        {
            try
            {
                _database?.Close();
            }
            catch (Exception ex)
            {
                log.Warn($"Error during database cleanup : {ex.Message}", ex);
            }
            finally
            {
                _database = null;
                _masterKey = null;
                _folder = null;
            }
        }

        private void CheckPreconditions()
        {
            var props = GetFileProperties();
            var dbPath = props.DBPath;
            if (!File.Exists(dbPath))
                throw new FileNotFoundException($"KeePass database file not found : {dbPath}");
            if (string.IsNullOrEmpty(props.Secret) && string.IsNullOrEmpty(props.KeyPath))
                throw new InvalidOperationException("At least one credential (password, key file, or both) required");
        }

        private KeePassKeyStoreProperties GetFileProperties() =>
            Properties as KeePassKeyStoreProperties ?? throw new KeyStoreException("Missing KeePass properties");

        private static string SetFieldName(string name) =>
            Regex.Replace(name ?? "", @"[^\w\-]", "_", RegexOptions.NonBacktracking);

        public override async Task Close(bool secretCleanup)
        {
            try
            {
                if (_database?.IsOpen == true)
                {
                    _database.Close();
                    log.Info("KeePass database closed successfully.");
                }
            }
            catch (Exception ex)
            {
                log.Warn($"Failed to close KeePass database : {ex.Message}", ex);
            }
            finally
            {
                if (secretCleanup)
                    CleanupSecret();
                Cleanup();
            }
            await base.Close(secretCleanup).ConfigureAwait(false);
        }
    }
}