using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Serialization;
using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Channels;

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
                await Task.Run(() => _database.Open(ioc, _masterKey, null), new CancellationTokenSource(OPEN_TIMEOUT).Token); //TODO improve later
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
            if (identifier?.Id == null)
                return false;
            await CheckOpen();
            return _folder?.Entries?.Any(e => MatchesEntryTitle(e, identifier.Id)) ?? false;
        }

        public override async Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass)
        {
            await CheckOpen();
            log.Info(string.Format("Getting all key entries (class: `{0}`)...", keClass));
            IList<KeyEntryId> keyEntries = new List<KeyEntryId>();
            if (_folder != null && _folder.Entries != null)
                foreach (PwEntry entry in _folder.Entries)
                {
                    var keid = new KeyEntryId { Id = entry.Strings.ReadSafe(PwDefs.TitleField) };
                    if (keClass != null)
                    {
                        var ke = await Get(keid, keClass.Value);
                        if (ke != null)
                            keid = ke.Identifier;
                    }
                    keyEntries.Add(keid);
                }
            log.Info(string.Format("{0} key entries returned.", keyEntries.Count));
            return keyEntries;
        }

        public override async Task Create(IChangeKeyEntry keyEntry)
        {
            ArgumentNullException.ThrowIfNull(keyEntry);
            if (string.IsNullOrEmpty(keyEntry.Identifier.Id))
                throw new ArgumentException("KeyEntry ID required", nameof(keyEntry));
            if (await CheckKeyEntryExists(keyEntry.Identifier, keyEntry.KClass))
            {
                log.Error(string.Format($"A key entry with the same identifier `{keyEntry.Identifier}` already exists."));
                throw new KeyStoreException("A key entry with the same identifier already exists.");
            }
            await CheckOpen();
            var entry = CreateKeePassEntry(keyEntry);
            if (_folder != null && _database != null)
            {
                _folder.AddEntry(entry, true);
                _database.Modified = true;
                _database.Save(null);
            }
        }

        private string SerializeKeyEntry(IChangeKeyEntry keyEntry) =>
            JsonConvert.SerializeObject(keyEntry, _jsonSettings);

        private KeyEntry? DeserializeKeyEntry(string json)
        {
            return JsonConvert.DeserializeObject<KeyEntry>(json, _jsonSettings);
        }

        public override async Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            if (!await CheckKeyEntryExists(identifier, keClass))
            {
                log.Error(string.Format($"The key entry `{identifier}` doesn't exist."));
                throw new KeyStoreException("The key entry doesn't exist.");
            }
            await CheckOpen();
            var entry = _folder?.Entries?.FirstOrDefault(e => MatchesEntryTitle(e, identifier.Id ?? ""));
            if (entry == null)
                return null;
            var keyData = entry.Strings.ReadSafe(CUSTOM_KEYDATA_FIELD);
            if (string.IsNullOrEmpty(keyData)) return null;
            try
            {
                var keyEntry = DeserializeKeyEntry(keyData);
                if (keyEntry != null)
                {
                    keyEntry.Identifier = identifier;
                    OnKeyEntryRetrieved(keyEntry);
                }
                log.Info(string.Format($"Key entry `{identifier}` retrieved."));
                return keyEntry;
            }
            catch (Exception ex)
            {
                log.Warn($"Failure when deserializing key entry {identifier.Id}: {ex.Message}");
                return null;
            }
        }

        private static bool MatchesEntryTitle(PwEntry entry, string title) =>
            string.Equals(entry.Strings.ReadSafe(PwDefs.TitleField), title, StringComparison.OrdinalIgnoreCase);

        public override async Task Update(IChangeKeyEntry change, bool ignoreIfMissing)
        {
            ArgumentNullException.ThrowIfNull(change);
            if (string.IsNullOrEmpty(change.Identifier.Id))
                throw new ArgumentException("KeyEntry ID required", nameof(change));
            await CheckOpen();
            var existing = FindEntryByTitle(change.Identifier.Id ?? throw new ArgumentException("ID required", nameof(change)));
            if (existing == null)
            {
                if (ignoreIfMissing) return;
                throw new KeyNotFoundException($"Key entry not found: {change.Identifier}");
            }
            UpdateKeePassEntry(existing, change);
            OnKeyEntryUpdated(change);
            log.Info(string.Format("Key entry `{0}` updated.", change.Identifier));

        }

        private PwEntry CreateKeePassEntry(IChangeKeyEntry keyEntry)
        {
            PwEntry entry = new PwEntry(true, true) { Uuid = new PwUuid(true) };
            entry.Strings.Set(PwDefs.UserNameField, new ProtectedString(true, keyEntry.Identifier.Label));
            entry.Strings.Set(PwDefs.TitleField, new ProtectedString(true, keyEntry.Identifier.Id));
            entry.Strings.Set(CUSTOM_KEYDATA_FIELD, new ProtectedString(true, SerializeKeyEntry(keyEntry)));
            return entry;
        }

        private void UpdateKeePassEntry(PwEntry entry, IChangeKeyEntry keyEntry)
        {
            entry.Strings.Set(PwDefs.TitleField, new ProtectedString(true, keyEntry.Identifier.Label));
            entry.Strings.Set(PwDefs.UserNameField, new ProtectedString(true, keyEntry.Identifier.Id));
            entry.Strings.Set(CUSTOM_KEYDATA_FIELD, new ProtectedString(true, SerializeKeyEntry(keyEntry)));
        }

        protected new void OnKeyEntryUpdated(IChangeKeyEntry keyEntry)
        {
            KeyEntryUpdated?.Invoke(this, keyEntry);
        }

        public override async Task Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing)
        {
            ArgumentNullException.ThrowIfNull(identifier);
            if (string.IsNullOrEmpty(identifier.Id))
                return;
            await CheckOpen();
            var exists = await CheckKeyEntryExists(identifier, keClass);
            var entry = FindEntryByTitle(identifier.Id ?? throw new ArgumentException("ID required", nameof(identifier)));
            if (!exists && entry == null && !ignoreIfMissing)
            {
                if (ignoreIfMissing) return;
                throw new KeyNotFoundException($"Key entry '{identifier.Id}' not found");
            }
            if (_folder == null)
                return;
            bool del = _folder.Entries.Remove(entry);
            if (!del)
                return;
            _database!.DeletedObjects.Add(new PwDeletedObject(entry?.Uuid, DateTime.Now));
            _database.Modified = true;
            _database.Save(null);
            log.Info($"Deleted key entry: {identifier.Id}");
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
        private PwEntry? FindEntryByTitle(string? title) //TODO refactor later
        {
            if (string.IsNullOrWhiteSpace(title)) return null;
            return _folder?.Entries?.FirstOrDefault(e => MatchesEntryTitle(e, title));
        }

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

        private async Task CheckOpen()
        {
            if (_database == null || _database?.IsOpen == false)
                await Open();
        }

        private void Cleanup()
        {
            try
            {
                _database?.Close();
                _database = null;
            }
            catch
            {

            }
            _masterKey = null;
            _folder = null;
        }


        private void CheckPreconditions()
        {
            //TODO later check config key file (if used)
            if (!File.Exists(GetFileProperties().DBpath))
                throw new FileNotFoundException($"KeePass database file not found: {GetFileProperties().DBpath}");

            if (string.IsNullOrEmpty(Properties?.Secret))
                throw new InvalidOperationException("Master password or key file required");
        }

        public KeePassKeyStoreProperties GetFileProperties()
        {
            var p = Properties as KeePassKeyStoreProperties;
            return p ?? throw new KeyStoreException("Missing File key store properties.");
        }

    }
}