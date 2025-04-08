using System.Reflection.Metadata;
using System.Runtime.Remoting;
using Vanara.PInvoke;
using static Vanara.PInvoke.NCrypt;

namespace Leosac.KeyManager.Library.KeyStore.CNG
{
    public class CNGKeyStore : KeyStore
    {
#pragma warning disable CA1416 // Validate platform compatibility
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        SafeNCRYPT_PROV_HANDLE _phProvider = SafeNCRYPT_PROV_HANDLE.Null;

        public override string Name => "CNG";

        public override bool CanCreateKeyEntries => true;

        public override bool CanDeleteKeyEntries => true;

        public override IEnumerable<KeyEntryClass> SupportedClasses
        {
            get => [KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric];
        }

        public static string?[] GetExistingProviders()
        {
            uint pdwProviderCount = 0;
            SafeNCryptBuffer ppProviderList;
            NCryptEnumStorageProviders(out pdwProviderCount, out ppProviderList, NCryptUIFlags.NCRYPT_SILENT_FLAG);

            var providers = new List<string>();
            if (!ppProviderList.IsNull)
            {
                var providerNames = ppProviderList.ToArray<NCryptProviderName>(pdwProviderCount);
                if (providerNames != null)
                {
                    foreach (var current in providerNames)
                    {
                        providers.Add(current.pszName);
                    }
                }
            }

            return (providers.Count > 0) ? [.. providers] : ["", "Microsoft Software Key Storage Provider", "Microsoft Smart Card Key Storage Provider", "Microsoft Platform Crypto Provider"];
        }

        public override async Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            SafeNCRYPT_KEY_HANDLE phKey;
            var r = await CheckKeyEntryExists(identifier, keClass, out phKey);
            if (r)
            {
                NCryptFreeObject(phKey.DangerousGetHandle());
            }
            return r;
        }

        public Task<bool> CheckKeyEntryExists(KeyEntryId identifier, out SafeNCRYPT_KEY_HANDLE phKey)
        {
            return CheckKeyEntryExists(identifier, null, out phKey);
        }

        public Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass? keClass, out SafeNCRYPT_KEY_HANDLE phKey)
        {
            if (_phProvider.IsInvalid || _phProvider.IsClosed)
            {
                throw new KeyStoreException("Invalid key storage provider handle.");
            }

            if (identifier.Handle != null && identifier.Handle is SafeNCRYPT_KEY_HANDLE h)
            {
                phKey = h;
                return Task.FromResult(true);
            }

            if (!string.IsNullOrEmpty(identifier.Label))
            {
                log.Warn("KeyEntry label specified but such key resolution is not supported by the key store type.");
            }

            if (!string.IsNullOrEmpty(identifier.Id))
            {
                var r = NCryptOpenKey(_phProvider, out phKey, identifier.Id, 0, GetCNGProperties().Scope == CNGScope.Machine ? OpenKeyFlags.NCRYPT_MACHINE_KEY_FLAG : 0);
                if (r != HRESULT.S_OK)
                {
                    log.Error(string.Format("NCryptOpenKey failed with code: {0}", r));
                }
                return Task.FromResult(true);
            }

            phKey = SafeNCRYPT_KEY_HANDLE.Null;
            return Task.FromResult(false);
        }

        public override Task Close(bool secretCleanup = true)
        {
            log.Info("Closing the key store...");

            if (!_phProvider.IsNull && !_phProvider.IsClosed)
            {
                var r = NCryptFreeObject(_phProvider.DangerousGetHandle());
                if (r != HRESULT.S_OK)
                {
                    throw new KeyStoreException(string.Format("NCryptFreeObject failed with code: {0}", r));
                }
            }
            _phProvider = SafeNCRYPT_PROV_HANDLE.Null;

            log.Info("Key Store closed.");
            return base.Close(secretCleanup);
        }

        public override async Task Create(IChangeKeyEntry change)
        {
            log.Info(string.Format("Creating key entry `{0}`...", change.Identifier));

            if (change is KeyEntry entry && entry.Variant != null && entry.Variant.KeyContainers.Count > 0)
            {
                throw new NotImplementedException();
            }
            else if (change is KeyEntryCryptogram cryptogram)
            {
                var wrappingKey = cryptogram.WrappingKey;
                if (wrappingKey == null)
                {
                    wrappingKey = Options?.WrappingKey;
                }
                if (wrappingKey == null || !wrappingKey.KeyId.IsConfigured())
                {
                    log.Error("Wrapping Key Entry Identifier parameter is expected.");
                    throw new KeyStoreException("Wrapping Key Entry Identifier parameter is expected.");
                }

                if (!await CheckKeyEntryExists(wrappingKey.KeyId, change.KClass))
                {
                    log.Error(string.Format("The key entry `{0}` doesn't exist.", wrappingKey.KeyId));
                    throw new KeyStoreException("The key entry doesn't exist.");
                }

                throw new NotSupportedException();
            }

            log.Info(string.Format("Key entry `{0}` created.", change.Identifier));
        }

        public override Task<KeyEntryId> Generate(KeyEntryId? identifier, KeyEntryClass keClass)
        {
            if (identifier == null)
            {
                identifier = new KeyEntryId();
            }

            var keyEntry = new CNGKeyEntry
            {
                Identifier = identifier
            };

            if (keClass == KeyEntryClass.Symmetric)
            {
                keyEntry.Variant = keyEntry.GetAllVariants(keClass).FirstOrDefault(v => v.Name == "AES128");
            }
            else
            {
                log.Error(string.Format("The key store doesn't support key entry generation without specifing the target type for class `{0}`.", keClass));
                throw new NotImplementedException();
            }
            return Generate(keyEntry);
        }

        public override async Task Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing)
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
                throw new NotImplementedException();
                log.Info(string.Format("Key entry `{0}` deleted.", identifier));
            }
        }

        public override async Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            log.Info(string.Format("Getting key entry `{0}`...", identifier));
            CNGKeyEntry? ke = null;

            if (!string.IsNullOrEmpty(identifier.Id))
            {
                throw new NotImplementedException();
            }

            return ke;
        }

        public override async Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass)
        {
            log.Info(string.Format("Getting all key entries (class: `{0}`)...", keClass));
            var entries = new List<KeyEntryId>();

            throw new NotImplementedException();

            log.Info(string.Format("{0} key entries returned.", entries.Count));
            return entries;
        }

        public CNGKeyStoreProperties GetCNGProperties()
        {
            var p = Properties as CNGKeyStoreProperties;
            return p ?? throw new KeyStoreException("Missing CNG key store properties.");
        }

        public override Task Open()
        {
            log.Info("Opening the key store...");

            var p = GetCNGProperties();

            var provider = GetCNGProperties().StorageProvider;
            var r = NCryptOpenStorageProvider(out _phProvider, string.IsNullOrEmpty(provider) ? null : provider);
            if (r != HRESULT.S_OK)
            {
                throw new KeyStoreException(string.Format("NCryptOpenStorageProvider failed with code: {0}", r));
            }

            log.Info("Key Store opened.");
            return Task.CompletedTask;
        }

        public override async Task Update(IChangeKeyEntry change, bool ignoreIfMissing)
        {
            log.Info(string.Format("Updating key entry `{0}`...", change.Identifier));

            if (!await CheckKeyEntryExists(change.Identifier, change.KClass))
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", change.Identifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            if (change is KeyEntry entry)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }

            log.Info(string.Format("Key entry `{0}` updated.", change.Identifier));
        }

        public override KeyEntry? GetDefaultKeyEntry(KeyEntryClass keClass)
        {
            return base.GetDefaultKeyEntry(keClass) ?? new CNGKeyEntry(keClass);
        }
#pragma warning restore CA1416 // Validate platform compatibility
    }
}
