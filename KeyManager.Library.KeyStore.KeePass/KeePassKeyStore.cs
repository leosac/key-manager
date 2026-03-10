using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Serialization;
using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace Leosac.KeyManager.Library.KeyStore.KeePass
{
    public class KeePassKeyStore : KeyStore
    {
        private static readonly TimeSpan OPEN_TIMEOUT = TimeSpan.FromSeconds(30);
        private const string CUSTOM_KEYDATA_FIELD = "KeyData_Leosac";
        private const string VARIANT_NAME_FIELD = "KeyVariantName_Leosac";
        private const string KEY_VERSION_FIELD = "KeyVersion_Leosac";
        private const string KEY_VALUE_FIELD = "KeyValue_Leosac";
        private const string DATE_MODIFICATION_FIELD = "LastModification_Leosac"; //Last modification from LKM

        private const string JSON_PROPERTIES = "Properties";
        private const string JSON_VARIANT = "Variant";
        private const string JSON_NAME = "Name";

        public override string Name => "KeePass";
        private PwDatabase? _database;
        private CompositeKey? _masterKey;

        private PwGroup? _folder;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(KeePassKeyStore));

        public override bool CanCreateKeyEntries => true;
        public override bool CanDeleteKeyEntries => true;
        public override bool CanUpdateKeyEntries => true;
        public bool IgnoreIncompleteEntries { get; set; }
        private static readonly KeyEntryClass[] _supported = { KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric };

        public override IEnumerable<KeyEntryClass> SupportedClasses => _supported;

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
                log.Error($"Failed to open KeePass .kdbx file : {ex.Message}", ex);
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
            return root;
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
            var id = identifier.Id;
            await CheckOpen().ConfigureAwait(false);
            var entry = FindEntryByTitle(id) ?? throw new KeyStoreException($"Key entry '{id}' doesn't exist.");
            var metadataJson = GetSafeString(entry, CUSTOM_KEYDATA_FIELD);
            if (string.IsNullOrEmpty(metadataJson))
            {
                log.Warn($"No metadata found for entry `{id}`");
                return null;
            }
            try
            {
                var obj = JObject.Parse(metadataJson);
                if (!obj.TryGetValue("KClass", out var classToken))
                {
                    log.Warn($"Missing KClass for `{id}`");
                    return null;
                }
                var _kclass = classToken.ToString();
                if (!Enum.TryParse(_kclass, out KeyEntryClass kclass) ||
                    kclass != keClass)
                {
                    log.Warn($"KClass mismatch for `{id}` : expected {keClass}, got {_kclass}");
                    return null;
                }
                if (!obj.TryGetValue(JSON_PROPERTIES, out _))
                {
                    log.Warn("Adding default Properties to metadata");
                    obj[JSON_PROPERTIES] = new JObject();
                }
                obj[JSON_VARIANT] = BuildVariant(entry);
                var keyEntry = CreateFromJson(obj, kclass, identifier);
                if (keyEntry == null)
                {
                    log.Warn($"Failed to create KeyEntry for `{id}`");
                    return null;
                }
                PopulateKeyEntry(keyEntry, obj, entry);
                var fallbackKeyMaterial = keyEntry.Variant?.KeyContainers?.FirstOrDefault()?.Key?.Materials?.FirstOrDefault();
                if (fallbackKeyMaterial != null && string.IsNullOrEmpty(fallbackKeyMaterial.Value))
                {
                    var password = GetSafeString(entry, PwDefs.PasswordField);
                    fallbackKeyMaterial.Value = string.IsNullOrEmpty(password) ? string.Empty : password;
                }
                OnKeyEntryRetrieved(keyEntry);
                log.Info($"Key entry `{id}` retrieved successfully.");
                return keyEntry;
            }
            catch (JsonException ex)
            {
                log.Warn($"JSON deserialization failed for `{id}` : {ex.Message}", ex);
                return null;
            }
            catch (Exception ex)
            {
                log.Warn($"Unexpected error deserializing key entry `{id}` : {ex.Message}", ex);
                return null;
            }
        }

        private static JObject BuildVariant(PwEntry entry)
        {
            var vName = GetSafeString(entry, VARIANT_NAME_FIELD) ?? "KeyVariant";
            var versionFields = entry.Strings
                .GetKeys()
                .Where(k => k.StartsWith(KEY_VERSION_FIELD, StringComparison.OrdinalIgnoreCase))
                .OrderBy(k => k, StringComparer.OrdinalIgnoreCase);
            var containers = new JArray();
            var failedFields = new List<string>();
            int index = 0;
            foreach (var field in versionFields)
            {
                var versionJson = GetSafeString(entry, field);
                if (string.IsNullOrWhiteSpace(versionJson))
                {
                    index++;
                    continue;
                }
                try
                {
                    var versionObj = JObject.Parse(versionJson);
                    var name = versionObj[JSON_NAME]?.ToString() ?? $"KeyVersion_{index}";
                    var keyObj = versionObj["Key"] is JObject key
                        ? (JObject)key.DeepClone()
                        : CreateDefaultKeyObject();
                    var container = new JObject
                    {
                        ["$type"] = "Leosac.KeyManager.Library.KeyStore.KeyVersion, KeyManager.Library",
                        [JSON_NAME] = name,
                        ["Key"] = keyObj
                    };
                    if (versionObj["Version"] is JToken versionToken)
                        container["Version"] = versionToken.Value<uint>();
                    containers.Add(container);
                }
                catch (JsonException ex)
                {
                    failedFields.Add(field);
                    log.Warn($"Failed to parse {field} : {ex.Message}", ex);
                }
                index++;
            }
            if (failedFields.Count > 0)
            {
                log.Warn($"Variant skipped {failedFields.Count} malformed field(s) : {string.Join(", ", failedFields)}");
            }
            return new JObject
            {
                [nameof(KeyEntryVariant.Name)] = vName,
                [nameof(KeyEntryVariant.KeyContainers)] = containers
            };
        }

        private static JObject CreateDefaultKeyObject()
        {
            return new JObject
            {
                ["KeySize"] = 16u,
                ["Tags"] = new JArray(),
                ["Materials"] = new JArray(),
                ["Link"] = new JObject
                {
                    ["KeyIdentifier"] = new JObject { ["Id"] = "" },
                    ["DivInput"] = new JArray()
                }
            };
        }

        private static KeyEntry? CreateFromJson(JObject obj, KeyEntryClass kclass, KeyEntryId identifier)
        {
            if (obj.TryGetValue(JSON_PROPERTIES, out var propToken) && propToken is JObject propObj &&
                propObj.TryGetValue("$type", out var typeToken))
            {
                var typeName = typeToken.Value<string>();
                if (!string.IsNullOrEmpty(typeName))
                {
                    var factory = KeyEntryFactory.GetFactoryFromKeyEntryType(typeName);
                    if (factory != null)
                    {
                        var keyEntry = factory.CreateKeyEntry();
                        keyEntry.Identifier = identifier;
                        return keyEntry;
                    }
                }
            }
            return CreateFallback(kclass, identifier); // Fallback
        }

        private static KeyEntry? CreateFallback(KeyEntryClass kclass, KeyEntryId identifier)
        {
            var ke = KeyEntryFactory.GetFactoryFromBestMatch(kclass)?.CreateKeyEntry();
            if (ke != null)
            {
                ke.Identifier = identifier;
            }
            return ke;
        }

        private static bool GetLabel(PwEntry entry, out string? label)
        {
            label = entry.Strings.GetSafe(PwDefs.UserNameField)?.ReadString();
            return !string.IsNullOrWhiteSpace(label);
        }

        private static void PopulateKeyEntry(KeyEntry keyEntry, JObject obj, PwEntry entry)
        {
            var props = obj[JSON_PROPERTIES] as JObject;
            var variant = obj[JSON_VARIANT] as JObject;
            SetProperties(keyEntry, props);
            SetVariant(keyEntry, variant, entry);
        }

        private static void SetProperties(KeyEntry keyEntry, JObject? obj)
        {
            if (obj == null)
            {
                log.Warn("Properties is null : using default properties");
                SetDefaultProperties(keyEntry);
                return;
            }
            try
            {
                var prop = obj.ToObject<KeyEntryProperties>(CachedSerializer);
                if (prop is null)
                {
                    log.Warn("Properties deserialization returned null : using default properties");
                    SetDefaultProperties(keyEntry);
                    return;
                }
                var propFactory = KeyEntryFactory.GetFactoryFromPropertyType(prop.GetType());
                keyEntry.Properties = propFactory?.CreateKeyEntryProperties(obj.ToString(Formatting.None)) ?? prop;
                log.Info($"Properties set : {keyEntry.Properties.GetType().Name}");
            }
            catch (Exception ex)
            {
                log.Error($"Operation failed : {ex.Message} : using default properties");
                SetDefaultProperties(keyEntry);
            }
        }

        private static void SetDefaultProperties(KeyEntry keyEntry) // Fallback for deserialization errors
        {
            try
            {
                var factory = KeyEntryFactory.GetFactoryFromKeyEntryType(keyEntry.GetType());
                if (factory != null)
                {
                    keyEntry.Properties = factory.CreateKeyEntryProperties();
                    log.Debug($"Default properties created");
                }
                else
                {
                    log.Warn($"Cannot found factory from key entry of type '{keyEntry.GetType().Name}'");
                    keyEntry.Properties = null;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed to create default properties for {keyEntry.GetType().Name} : {ex.Message}");
                keyEntry.Properties = null;
            }
        }

        private static void SetVariant(KeyEntry keyEntry, JObject? variant, PwEntry entry)
        {
            if (variant == null) return;
            var variantObj = keyEntry.Variant ?? new KeyEntryVariant
            {
                KeyContainers = new ObservableCollection<KeyContainer>()
            };
            variantObj.Name = variant[JSON_NAME]?.ToString() ?? string.Empty;
            if (variant["KeyContainers"] is not JArray containers) return;
            log.Info($"Found {containers.Count} containers");
            variantObj.KeyContainers.Clear();
            for (int i = 0; i < containers.Count; i++)
            {
                if (containers[i] is not JObject kcObj) continue;
                string name = kcObj[JSON_NAME]?.ToString() ?? $"Key {i + 1}";
                KeyContainer container = kcObj.ContainsKey("Version")
                    ? new KeyVersion(name, kcObj["Version"]!.Value<byte>())
                    : new KeyContainer(name);
                if (kcObj["Key"] is JObject keyObj)
                {
                    var key = container.Key;
                    key.KeySize = (ushort)(keyObj["KeySize"]?.Value<int>() ?? 16);
                    key.Tags = keyObj["Tags"]?.ToObject<ObservableCollection<string>>() ?? new ObservableCollection<string>();
                    key.Link = keyObj["Link"]?.ToObject<KeyLink>() ?? new KeyLink();
                    if (keyObj["Materials"] is JArray materialsArray && materialsArray.Count > 0)
                    {
                        try
                        {
                            var materials = materialsArray.ToObject<ObservableCollection<KeyMaterial>>(CachedSerializer)
                                            ?? new ObservableCollection<KeyMaterial>();
                            if (i > 0 && materials.Count > 0)
                            {
                                string fieldName = $"KeyValue_Leosac_{SetFieldName(name)}";
                                materials[0].Value = GetSafeString(entry, fieldName) ?? "";
                                log.Info($"Key {name} : restored '{Short(materials[0].Value)}...' from {fieldName}");
                            }
                            key.Materials = materials;
                            log.Info($"Key {name} : '{Short(key.Materials.FirstOrDefault()?.Value ?? "empty")}...'");
                        }
                        catch (Exception ex)
                        {
                            log.Error($"Failed to deserialize materials for {name} : {ex.Message}");
                        }
                    }
                }
                variantObj.KeyContainers.Add(container);
            }
            keyEntry.Variant = variantObj;
            if (variantObj.KeyContainers.Count > 0)
            {
                var pw = GetSafeString(entry, PwDefs.PasswordField);
                if (!string.IsNullOrEmpty(pw))
                {
                    variantObj.KeyContainers[0].Key.SetAggregatedValueAsString(pw);
                    log.Info($"Key A password override : '{Short(pw)}...'");
                }
            }
            log.Info($"Final variant : {variantObj.KeyContainers.Count} containers loaded");
        }

        private static string Short(string s) => string.IsNullOrEmpty(s) ? "" : (s.Length <= 8 ? s : s[..8]);

        private static string GetSafeString(PwEntry entry, string field) =>
            entry?.Strings?.GetSafe(field)?.ReadString() ?? string.Empty;

        private static bool MatchesEntryTitle(PwEntry entry, string title) =>
            string.Equals(entry?.Strings?.ReadSafe(PwDefs.TitleField), title, StringComparison.OrdinalIgnoreCase);


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
            ArgumentNullException.ThrowIfNull(entry);
            ArgumentNullException.ThrowIfNull(keyEntry);
            if (keyEntry is not KeyEntry keyEntryInstance)
                throw new ArgumentException("KeyEntry instance not supported.", nameof(keyEntry));
            try
            {
                var metadataJson = JsonConvert.SerializeObject(new
                {
                    keyEntryInstance.KClass,
                    keyEntryInstance.Properties
                }, SerializerSettings);
                entry.Strings.Set(CUSTOM_KEYDATA_FIELD, new ProtectedString(true, metadataJson));
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
            if (entry == null || keyEntry == null) return;
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
            var firstContainer = variant?.KeyContainers?.FirstOrDefault();
            return firstContainer?.Key?.Materials?.FirstOrDefault(m => m.IsRealKeyMateriel)?.Value;
        }

        private static void ResetKeyVersionFields(PwEntry entry)
        {
            if (entry?.Strings == null) return;
            foreach (var key in entry.Strings.GetKeys()
                .Where(k => k.StartsWith(KEY_VERSION_FIELD, StringComparison.OrdinalIgnoreCase)).ToList())
            {
                entry.Strings.Remove(key);
            }
        }

        private static void StoreKeyVersions(PwEntry entry, IEnumerable<KeyContainer>? containers)
        {
            if (entry?.Strings == null || containers?.Any() != true) return;
            int index = 0;
            foreach (var container in containers.Where(c => c?.Key != null))
            {
                var keyContainer = container.Key!;
                string safeName = SetFieldName(container.Name ?? $"V{index}");
                string versionFieldName = $"{KEY_VERSION_FIELD}_{safeName}";
                string valueFieldName = $"{KEY_VALUE_FIELD}_{safeName}";
                JArray BuildMaterials()
                {
                    return new JArray(
                        (keyContainer.Materials ?? Enumerable.Empty<KeyMaterial>())
                        .Select(m => new JObject
                        {
                            ["OverrideSize"] = m.OverrideSize
                        })
                    );
                }
                var serializedKey = new JObject(new JProperty(JSON_NAME, container.Name ?? "Undefined"));
                if (container is KeyVersion kv)
                    serializedKey.Add("Version", kv.Version);
                serializedKey.Add("Key", new JObject(
                    new JProperty("KeySize", keyContainer.KeySize),
                    new JProperty("Tags", new JArray((keyContainer.Tags ?? new ObservableCollection<string>()).ToArray())),
                    new JProperty("Materials", BuildMaterials()),
                    new JProperty("Link", JObject.FromObject(keyContainer.Link ?? new KeyLink()))
                ));
                entry.Strings.Set(versionFieldName, new ProtectedString(true, serializedKey.ToString(Formatting.Indented)));
                if (container is KeyVersion && index > 0)
                {
                    var firstMaterialValue = keyContainer.Materials?.FirstOrDefault(m => m.IsRealKeyMateriel)?.Value ?? "";
                    entry.Strings.Set(valueFieldName, new ProtectedString(true, firstMaterialValue));
                    log.Debug(firstMaterialValue.Length > 0
                        ? $"Stored key value for {container.Name} in {valueFieldName}: {Short(firstMaterialValue)}..."
                        : $"Created empty KeyValue field : {valueFieldName}");
                }
                else
                {
                    entry.Strings.Remove(valueFieldName);
                }

                index++;
            }
        }

        private static string BuildNotes(KeyEntry keyEntry)
        {
            var variant = keyEntry.Variant;
            if (variant?.KeyContainers == null)
                return $"Key Type : Unknown{Environment.NewLine}Class : {keyEntry.KClass}";
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
            var containerCount = variant.KeyContainers.Count;
            if (containerCount > 1)
                sb.Append($"Versions : {containerCount}").Append(Environment.NewLine);
            for (int i = 0; i < Math.Min(3, containerCount); i++)
            {
                var container = variant.KeyContainers[i];
                var name = container.Name ?? $"V{i + 1}";
                var size = container.Key.KeySize * 8;
                var tagsPreview = string.Join("/", container.Key.Tags?.Take(2) ?? []);
                sb.Append($"  ↳ {name} : {size}b ({tagsPreview})").Append(Environment.NewLine); //UTF-8 symbol for pretty output
            }
            return sb.ToString().TrimEnd();
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
            var props = GetFileProperties();
            var key = new CompositeKey();
            var modeActions = new Dictionary<CredentialMode, Action>
            {
                [CredentialMode.PasswordOnly] = () =>
                {
                    if (string.IsNullOrEmpty(props.Secret))
                        throw new InvalidOperationException("Password is required for PasswordOnly mode.");
                    key.AddUserKey(new KcpPassword(props.Secret));
                },
                [CredentialMode.KeyFileOnly] = () =>
                {
                    if (string.IsNullOrEmpty(props.KeyPath) || !File.Exists(props.KeyPath))
                        throw new InvalidOperationException("Valid key file is required for KeyFileOnly mode.");
                    key.AddUserKey(new KcpKeyFile(props.KeyPath));
                },
                [CredentialMode.PasswordAndKey] = () =>
                {
                    if (string.IsNullOrEmpty(props.Secret))
                        throw new InvalidOperationException("Password is required for Password+KeyFile mode.");
                    if (string.IsNullOrEmpty(props.KeyPath) || !File.Exists(props.KeyPath))
                        throw new InvalidOperationException("Valid key file is required for Password+KeyFile mode.");
                    key.AddUserKey(new KcpPassword(props.Secret));
                    key.AddUserKey(new KcpKeyFile(props.KeyPath));
                }
            };
            if (!modeActions.TryGetValue(props.SelectedCredentialMode, out var buildAction))
                throw new InvalidOperationException("Invalid credential mode.");
            buildAction();
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

        private KeePassKeyStoreProperties GetFileProperties()
        {
            var props = Properties as KeePassKeyStoreProperties
                ?? throw new KeyStoreException("Missing KeePass properties");
            IgnoreIncompleteEntries = props.Silent;
            return props;
        }

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