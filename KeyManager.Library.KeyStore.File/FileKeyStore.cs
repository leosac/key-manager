using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;
using System.Text;

namespace Leosac.KeyManager.Library.KeyStore.File
{
    public class FileKeyStore : KeyStore
    {
        public const string LeosacKeyFileExtension = ".leok";

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public FileKeyStore()
        {
            Properties = new FileKeyStoreProperties();
            _jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                Formatting = Formatting.Indented
            };
        }

        private readonly JsonSerializerSettings _jsonSettings;

        public FileKeyStoreProperties GetFileProperties()
        {
            var p = Properties as FileKeyStoreProperties;
            return p ?? throw new KeyStoreException("Missing File key store properties.");
        }

        public override string Name => "File";

        public override bool CanCreateKeyEntries => true;

        public override bool CanDeleteKeyEntries => true;

        public override bool CanReorderKeyEntries => false;

        public override IEnumerable<KeyEntryClass> SupportedClasses
        {
            get => new KeyEntryClass[] { KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric };
        }

        public override Task Open()
        {
            log.Info(String.Format("Opening the key store `{0}`...", GetFileProperties().Fullpath));
            if (!System.IO.Directory.Exists(GetFileProperties().Fullpath))
            {
                if (CreateIfMissing)
                    System.IO.Directory.CreateDirectory(GetFileProperties().Fullpath);
                else
                {
                    log.Error(string.Format("Cannot open the key sore `{0}`.", GetFileProperties().Fullpath));
                    throw new KeyStoreException("Cannot open the key sore.");
                }
            }
            log.Info("Key store opened.");
            return Task.CompletedTask;
        }

        public override Task Close()
        {
            log.Info(string.Format("Closing the key store `{0}`...", GetFileProperties().Fullpath));
            log.Info("Key Store closed.");
            return Task.CompletedTask;
        }

        protected string GetKeyEntryFile(KeyEntryId identifier, KeyEntryClass keClass)
        {
            return System.IO.Path.Combine(GetFileProperties().Fullpath, identifier.Id + "." + keClass + LeosacKeyFileExtension);
        }

        public override Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            return Task.FromResult(System.IO.File.Exists(GetKeyEntryFile(identifier, keClass)));
        }

        public override async Task Create(IChangeKeyEntry change)
        {
            log.Info(string.Format("Creating key entry `{0}`...", change.Identifier));
            if (await CheckKeyEntryExists(change.Identifier, change.KClass))
            {
                log.Error(string.Format("A key entry with the same identifier `{0}` already exists.", change.Identifier));
                throw new KeyStoreException("A key entry with the same identifier already exists.");
            }

            var kefile = GetKeyEntryFile(change.Identifier, change.KClass);
            if (change is KeyEntry)
            {
                var serialized = JsonConvert.SerializeObject(change, typeof(KeyEntry), _jsonSettings);
                using var aes = System.Security.Cryptography.Aes.Create();
                aes.Key = GetWrappingKey();
                var data = aes.EncryptCbc(Encoding.UTF8.GetBytes(serialized), new byte[16], System.Security.Cryptography.PaddingMode.PKCS7);
                await System.IO.File.WriteAllBytesAsync(kefile, data);
            }
            else if (change is KeyEntryCryptogram cryptogram)
            {
                System.IO.File.WriteAllText(kefile, cryptogram.Value);
            }
            else
                throw new KeyStoreException("Unsupported `change` parameter.");

            log.Info(string.Format("Key entry `{0}` created.", change.Identifier));
        }

        public override async Task Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing = false)
        {
            log.Info(string.Format("Deleting key entry `{0}`...", identifier));
            var exists = await CheckKeyEntryExists(identifier, keClass);
            if (!exists && !ignoreIfMissing)
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", identifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            if (exists)
            {
                System.IO.File.Delete(GetKeyEntryFile(identifier, keClass));
                log.Info(string.Format("Key entry `{0}` deleted.", identifier));
            }
        }

        public override async Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            log.Info(string.Format("Getting key entry `{0}`...", identifier));
            if (!await CheckKeyEntryExists(identifier, keClass))
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", identifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            var encdata = await System.IO.File.ReadAllBytesAsync(GetKeyEntryFile(identifier, keClass));
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = GetWrappingKey();
            var data = aes.DecryptCbc(encdata, new byte[16], System.Security.Cryptography.PaddingMode.PKCS7);
            var keyEntry = JsonConvert.DeserializeObject<KeyEntry>(Encoding.UTF8.GetString(data), _jsonSettings);
            // Plugin context workaround
            if (keyEntry != null)
            {
                var factory = KeyEntryFactory.GetFactoryFromPropertyType(keyEntry.Properties?.GetType());
                if (factory != null)
                {
                    keyEntry.Properties = factory.CreateKeyEntryProperties(JsonConvert.SerializeObject(keyEntry.Properties));
                }
            }
            log.Info(string.Format("Key entry `{0}` retrieved.", identifier));
            return keyEntry;
        }

        private byte[] GetWrappingKey()
        {
            if (string.IsNullOrEmpty(Properties?.Secret))
            {
                return new byte[16];
            }

            var key = Convert.FromHexString(Properties.Secret);
            if (key.Length != 16 && key.Length != 32)
                throw new KeyStoreException("Wrong wrapping key length.");

            return key;
        }

        public override Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass = null)
        {
            log.Info(string.Format("Getting all key entries (class: `{0}`)...", keClass));
            IList<KeyEntryId> keyEntries = new List<KeyEntryId>();
            var filter = "*";
            if (keClass != null)
            {
                filter += "." + keClass.ToString()!.ToLower();
            }
            filter += LeosacKeyFileExtension;
            var files = System.IO.Directory.GetFiles(GetFileProperties().Fullpath, filter);
            foreach (var file in files)
            {
                string identifier = System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetFileNameWithoutExtension(file));
                keyEntries.Add(new KeyEntryId { Id = identifier });
            }
            log.Info(string.Format("{0} key entries returned.", keyEntries.Count));
            return Task.FromResult(keyEntries);
        }

        public override Task MoveDown(KeyEntryId identifier, KeyEntryClass keClass)
        {
            throw new NotImplementedException();
        }

        public override Task MoveUp(KeyEntryId identifier, KeyEntryClass keClass)
        {
            throw new NotImplementedException();
        }

        public override async Task Store(IList<IChangeKeyEntry> changes)
        {
            log.Info(string.Format("Storing `{0}` key entries...", changes.Count));
            foreach (var change in changes)
            {
                await Update(change, true);
            }
            log.Info("Key Entries storing completed.");
        }

        public override async Task Update(IChangeKeyEntry change, bool ignoreIfMissing = false)
        {
            log.Info(string.Format("Updating key entry `{0}`...", change.Identifier));
            await Delete(change.Identifier, change.KClass, ignoreIfMissing);
            await Create(change);
            OnKeyEntryUpdated(change);
            log.Info(string.Format("Key entry `{0}` updated.", change.Identifier));
        }

        public override async Task<string?> ResolveKeyEntryLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? divInput = null, KeyEntryId? wrappingKeyId = null, string? wrappingKeyContainerSelector = null)
        {
            string? result = null;
            log.Info(string.Format("Resolving key entry link with Key Entry Identifier `{0}`, Div Input `{1}`...", keyIdentifier, divInput));

            var keyEntry = await Get(keyIdentifier, keClass);
            if (keyEntry != null)
            {
                log.Info("Key entry link resolved.");
                if (wrappingKeyId != null)
                {
                    var wrappingKey = await GetKey(wrappingKeyId, KeyEntryClass.Symmetric, wrappingKeyContainerSelector);
                    if (wrappingKey != null)
                    {
                        // TODO: do something here to encipher the key value?
                        // The wrapping algorithm may be too close to the targeted Key Store
                        throw new NotSupportedException();
                    }
                    else
                    {
                        log.Error("Cannot resolve the wrapping key.");
                    }
                }
                else
                {
                    result = JsonConvert.SerializeObject(keyEntry, typeof(KeyEntry), _jsonSettings);
                }
            }
            else
            {
                log.Error("Cannot resolve the key entry link.");
            }

            log.Info("Key entry link completed.");
            return result;
        }

        public override async Task<string?> ResolveKeyLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? containerSelector, string? divInput = null)
        {
            string? result = null;
            log.Info(string.Format("Resolving key link with Key Entry Identifier `{0}`, Container Selector `{1}`, Div Input `{2}`...", keyIdentifier, containerSelector, divInput));

            if (!await CheckKeyEntryExists(keyIdentifier, keClass))
            {
                log.Error(string.Format("The key entry `{0}` do not exists.", keyIdentifier));
                throw new KeyStoreException("The key entry do not exists.");
            }

            var key = await GetKey(keyIdentifier, keClass, containerSelector);
            if (key != null)
            {
                log.Info("Key link resolved.");
                result = key.GetAggregatedValue<string>();
            }
            else
            {
                log.Error("Cannot resolve the key link.");
            }

            log.Info("Key link completed.");
            return result;
        }
    }
}
