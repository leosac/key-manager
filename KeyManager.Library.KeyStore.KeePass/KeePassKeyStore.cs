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

namespace Leosac.KeyManager.Library.KeyStore.KeePass
{
    public class KeePassKeyStore : KeyStore
    {
        private static readonly TimeSpan OPEN_TIMEOUT = TimeSpan.FromSeconds(30);
        private const string CUSTOM_KEYDATA_FIELD = "KeyData_Leosac";

        public override string Name => "KeePass";
        private PwDatabase? _database;
        private CompositeKey? _masterKey;

        private PwGroup? _folder; //TODO for now only root is considered

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(KeePassKeyStore));

        public override bool CanCreateKeyEntries => true;
        public override bool CanDeleteKeyEntries => true;
        public override bool CanUpdateKeyEntries => true;

        public new event EventHandler<IChangeKeyEntry>? KeyEntryUpdated;

        public KeePassKeyStore()
        {
            Properties = new KeePassKeyStoreProperties();
        }

        public override IEnumerable<KeyEntryClass> SupportedClasses =>
            [KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric];


        public override async Task Open()
        {
            try
            {
                CheckPreconditions();
                Cleanup();
                _database = new PwDatabase();
                _masterKey = BuildMasterKey();
                var ioc = new IOConnectionInfo { Path = GetFileProperties().DBpath };
                ValidateIOConnection(ioc);
                using var cts = new CancellationTokenSource(OPEN_TIMEOUT);
                await Task.Run(() => _database.Open(ioc, _masterKey, null), cts.Token).ConfigureAwait(false);
                CheckState();
                _folder = _database.RootGroup; //TODO change when root won't be considered as the only folder anymore
                log.Info($"KeePass .kdbx file opened successfully : {GetFileProperties().DBpath}");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to open KeePass .kdbx file : {ex.Message}", ex);
                Cleanup();
                throw new KeyStoreException("Failed to open KeePass database file.", ex);
            }
        }

        public override async Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            ArgumentNullException.ThrowIfNull(identifier);
            if (string.IsNullOrWhiteSpace(identifier.Id))
                return false;
            await CheckOpen().ConfigureAwait(false);
            return _folder?.Entries?.Any(e => MatchesEntryTitle(e, identifier.Id)) ?? false;
        }

        public override async Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass)
        {
            await CheckOpen().ConfigureAwait(false);
            log.Info(string.Format("Getting all key entries (class: `{0}`)...", keClass));
            IList<KeyEntryId> keyEntries = new List<KeyEntryId>();
            if (_folder?.Entries == null)
                return keyEntries;
            foreach (var entry in _folder.Entries)
            {
                var id = entry.Strings.ReadSafe(PwDefs.TitleField);
                if (string.IsNullOrWhiteSpace(id))
                    continue;
                var keyEntryId = new KeyEntryId { Id = id };
                if (GetLabel(entry, out var label) && !string.IsNullOrWhiteSpace(label))
                    keyEntryId.Label = label;

                keyEntries.Add(keyEntryId);
            }
            log.Info(string.Format("{0} key entries returned.", keyEntries.Count));
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
            log.Info($"Created key entry: {keyEntry.Identifier.Id}");
        }

        public override async Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            if (identifier == null || string.IsNullOrWhiteSpace(identifier.Id))
                throw new ArgumentException("Identifier required", nameof(identifier));
            await CheckOpen().ConfigureAwait(false);
            var entry = FindEntryByTitle(identifier.Id) ?? throw new KeyStoreException($"Key entry '{identifier}' doesn't exist.");
            var keyData = GetSafeString(entry, CUSTOM_KEYDATA_FIELD);
            if (string.IsNullOrEmpty(keyData))
            {
                log.Warn($"No key data found for entry: {identifier.Id}");
                return null;
            }
            try
            {
                var obj = JObject.Parse(keyData);
                var _kclass = obj["KClass"]?.ToString();
                if (!Enum.TryParse<KeyEntryClass>(_kclass, out var kclass) || kclass != keClass)
                {
                    log.Warn($"KClass mismatch for {identifier.Id}: expected {keClass}, got {_kclass}");
                    return null;
                }
                KeyEntry? keyEntry = CreateFromJson(obj, kclass, identifier);
                if (keyEntry == null)
                {
                    log.Warn($"Failed to create KeyEntry for {identifier.Id}");
                    return null;
                }
                var settings = KeyEntry.CreateJsonSerializerSettings();
                PopulateKeyEntry(keyEntry, obj, settings);
                OnKeyEntryRetrieved(keyEntry);
                log.Info($"Key entry `{identifier}` retrieved successfully.");
                return keyEntry;
            }
            catch (Exception ex)
            {
                log.Warn($"Failure when deserializing key entry {identifier.Id}: {ex.Message}", ex);
                return null;
            }
        }

        private KeyEntry? CreateFromJson(JObject obj, KeyEntryClass kclass, KeyEntryId identifier)
        {
            if (obj["Properties"]?["$type"] is JToken typeToken)
            {
                var propsTypeStr = typeToken.ToString();
                var keyEntryType = MapToKeyEntry(propsTypeStr, kclass);
                if (keyEntryType != null)
                {
                    var keyEntry = (KeyEntry?)Activator.CreateInstance(keyEntryType);
                    keyEntry!.Identifier = identifier;
                    return keyEntry;
                }
            }
            return kclass switch //Fallback
            {
                KeyEntryClass.Symmetric => new SAMSymmetricKeyEntry(),
                KeyEntryClass.Asymmetric => new AsymmetricPKCS11KeyEntry(),
                _ => null
            };
        }

        private static Type? MapToKeyEntry(string prop, KeyEntryClass kclass) =>
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
            label = null;
            var labelString = entry.Strings.GetSafe(PwDefs.UserNameField);
            if (labelString == null)
                return false;
            label = labelString.ReadString();
            return !string.IsNullOrWhiteSpace(label);
        }

        private static void PopulateKeyEntry(KeyEntry keyEntry, JObject obj, JsonSerializerSettings settings)
        {
            SetProperties(keyEntry, obj["Properties"] as JObject, settings);
            SetVariant(keyEntry, obj["Variant"] as JObject, settings);
        }

        private static void SetProperties(KeyEntry keyEntry, JObject? pJobj, JsonSerializerSettings settings)
        {
            if (pJobj == null)
                return;
            var json = pJobj.ToString(Formatting.None);
            var prop = JsonConvert.DeserializeObject<KeyEntryProperties>(json, settings);
            if (prop == null)
                return;
            var propFactory = KeyEntryFactory.GetFactoryFromPropertyType(prop.GetType());
            keyEntry.Properties = propFactory?.CreateKeyEntryProperties(json) ?? prop;
        }

        private static void SetVariant(KeyEntry keyEntry, JObject? variant, JsonSerializerSettings settings)
        {
            if (variant == null)
                return;
            keyEntry.Variant ??= new KeyEntryVariant();
            JsonConvert.PopulateObject(variant.ToString(Formatting.None), keyEntry.Variant, settings);
            SetContainers(keyEntry.Variant, variant["KeyContainers"] as JArray, settings);
        }

        private static void SetContainers(KeyEntryVariant variant, JArray? containers, JsonSerializerSettings settings)
        {
            if (containers?.HasValues != true)
                return;
            variant.KeyContainers.Clear();
            var serializer = JsonSerializer.Create(settings);
            foreach (var c in containers.OfType<JObject>())
            {
                var kc = c.ToObject<KeyContainer>(serializer);
                if (kc != null)
                    variant.KeyContainers.Add(kc);
            }
        }

        private static string GetSafeString(PwEntry entry, string fieldName) =>
            entry.Strings.GetSafe(fieldName) is ProtectedString ps ? ps.ReadString() : string.Empty;

        private static bool MatchesEntryTitle(PwEntry entry, string title) =>
            string.Equals(entry.Strings.ReadSafe(PwDefs.TitleField), title, StringComparison.OrdinalIgnoreCase);

        public override async Task Update(IChangeKeyEntry change, bool ignoreIfMissing)
        {
            ArgumentNullException.ThrowIfNull(change);
            if (string.IsNullOrEmpty(change.Identifier.Id))
                throw new ArgumentException("KeyEntry ID required", nameof(change));
            await CheckOpen().ConfigureAwait(false);
            var existing = FindEntryByTitle(change.Identifier.Id!);
            if (existing == null)
            {
                if (ignoreIfMissing) return;
                throw new KeyNotFoundException($"Key entry not found: {change.Identifier}");
            }
            UpdateKeePassEntry(existing, change);
            SaveDatabase();
            OnKeyEntryUpdated(change);
            log.Info($"Key entry `{change.Identifier}` updated.");
        }

        private PwEntry CreateKeePassEntry(IChangeKeyEntry keyEntry)
        {
            ArgumentNullException.ThrowIfNull(keyEntry);
            if (keyEntry is not KeyEntry concrete)
                throw new ArgumentException("Only concrete KeyEntry supported", nameof(keyEntry));
            var entry = new PwEntry(true, true) { Uuid = new PwUuid(true) };
            UpdateKeePassEntry(entry, concrete);
            return entry;
        }

        private void UpdateKeePassEntry(PwEntry entry, IChangeKeyEntry keyEntry)
        {
            ArgumentNullException.ThrowIfNull(keyEntry);
            var settings = KeyEntry.CreateJsonSerializerSettings();
            if (keyEntry is KeyEntry concreteEntry)
            {
                var serializable = new
                {
                    concreteEntry.KClass,
                    concreteEntry.Identifier,
                    Properties = concreteEntry.Properties,
                    Variant = concreteEntry.Variant
                };
                var json = JsonConvert.SerializeObject(serializable, concreteEntry.GetType(), settings);
                entry.Strings.Set(CUSTOM_KEYDATA_FIELD, new ProtectedString(true, json));
            }
            entry.Strings.Set(PwDefs.TitleField, new ProtectedString(true, keyEntry.Identifier.Id!));
            if (keyEntry.Identifier.Label is { } label && !string.IsNullOrWhiteSpace(label))
                entry.Strings.Set(PwDefs.UserNameField, new ProtectedString(true, label));
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
                throw new InvalidOperationException("Database or folder not initialized.");
            var del = _folder!.Entries.Remove(entry);
            if (!del)
            {
                log.Warn($"Failed to remove entry from folder : {identifier.Id}");
                return;
            }
            _database.DeletedObjects.Add(new PwDeletedObject(entry.Uuid, DateTime.Now));
            SaveDatabase();
            log.Info($"Deleted key entry : {identifier.Id}");
        }

        private CompositeKey BuildMasterKey()
        {
            //TODO consider key file

            var key = new CompositeKey();
            if (!string.IsNullOrEmpty(Properties?.Secret))
                key.AddUserKey(new KcpPassword(Properties?.Secret));

            return key.UserKeyCount switch
            {
                1 => key,
                0 => throw new InvalidOperationException("Master password OR key file required"),
                _ => throw new InvalidOperationException("Exactly one master key component allowed")
            };
        }
        private PwEntry? FindEntryByTitle(string title) =>
            string.IsNullOrWhiteSpace(title) ? null : _folder?.Entries?.FirstOrDefault(e => MatchesEntryTitle(e, title));

        private void ValidateIOConnection(IOConnectionInfo ioc)
        {
            if (string.IsNullOrWhiteSpace(ioc.Path))
                throw new ArgumentException("Invalid IO connection path", nameof(ioc));
        }

        private void CheckState()
        {
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            if (!_database.IsOpen)
                throw new InvalidOperationException("Database not opened");
        }

        protected new void OnKeyEntryUpdated(IChangeKeyEntry keyEntry)
        {
            KeyEntryUpdated?.Invoke(this, keyEntry);
        }

        private async Task CheckOpen()
        {
            if (_database?.IsOpen != true)
                await Open().ConfigureAwait(false);
        }

        private static void ValidateIdentifier(KeyEntryId identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier?.Id))
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
                log.Warn($"Error during database cleanup : {ex.Message}");
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
            //TODO later check config key file (if used)
            var dbPath = GetFileProperties().DBpath;
            if (!File.Exists(dbPath))
                throw new FileNotFoundException($"KeePass database file not found : {dbPath}");
            if (string.IsNullOrEmpty(Properties?.Secret))
                throw new InvalidOperationException("Master password or key file required");
        }
        private KeePassKeyStoreProperties GetFileProperties() =>
            Properties as KeePassKeyStoreProperties ?? throw new KeyStoreException("Missing KeePass properties");

    }
}