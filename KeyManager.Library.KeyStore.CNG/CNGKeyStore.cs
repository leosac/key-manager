using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Vanara.InteropServices;
using Vanara.PInvoke;
using static Vanara.PInvoke.NCrypt;

namespace Leosac.KeyManager.Library.KeyStore.CNG
{
    public class CNGKeyStore : KeyStore
    {
#pragma warning disable CA1416 // Validate platform compatibility
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        SafeNCRYPT_PROV_HANDLE _phProvider = SafeNCRYPT_PROV_HANDLE.Null;

        private static byte[] s_cipherKeyBlobPrefix = {
            // NCRYPT_KEY_BLOB_HEADER.cbSize (16)
            0x10, 0x00, 0x00, 0x00,
            // NCRYPT_KEY_BLOB_HEADER.dwMagic (NCRYPT_CIPHER_KEY_BLOB_MAGIC (0x52485043))
            0x43, 0x50, 0x48, 0x52,
            // NCRYPT_KEY_BLOB_HEADER.cbAlgName (8)
            0x08, 0x00, 0x00, 0x00,
            // NCRYPT_KEY_BLOB_HEADER.cbKeyData (to be determined)
            0x00, 0x00, 0x00, 0x00,
            // UTF16-LE "AES\0"
            0x41, 0x00, 0x45, 0x00, 0x53, 0x00, 0x00, 0x00,
            // BCRYPT_KEY_DATA_BLOB_HEADER.dwMagic (BCRYPT_KEY_DATA_BLOB_MAGIC (0x4D42444B))
            0x4B, 0x44, 0x42, 0x4D,
            // BCRYPT_KEY_DATA_BLOB_HEADER.dwVersion (1)
            0x01, 0x00, 0x00, 0x00,
            // BCRYPT_KEY_DATA_BLOB_HEADER.cbKeyData (to be determined)
            0x00, 0x00, 0x00, 0x00
        };

        public override string Name => "CNG";

        public override bool CanCreateKeyEntries => true;

        public override bool CanUpdateKeyEntries => false;

        public override bool CanDeleteKeyEntries => true;

        public override bool CanDefineKeyEntryLabel => false;

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
                var r = NCryptOpenKey(_phProvider, out phKey, identifier.Id, 0, GetOpenKeyFlagScope());
                if (r != HRESULT.S_OK)
                {
                    log.Error(string.Format("NCryptOpenKey failed with code: {0}", r));
                }
                return Task.FromResult(true);
            }

            phKey = SafeNCRYPT_KEY_HANDLE.Null;
            return Task.FromResult(false);
        }

        private OpenKeyFlags GetOpenKeyFlagScope()
        {
            return GetCNGProperties().Scope == CNGScope.Machine ? OpenKeyFlags.NCRYPT_MACHINE_KEY_FLAG : 0;
        }

        private CreatePersistedFlags GetCreateKeyFlagScope()
        {
            return GetCNGProperties().Scope == CNGScope.Machine ? CreatePersistedFlags.NCRYPT_MACHINE_KEY_FLAG : 0;
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

            if (_phProvider.IsInvalid || _phProvider.IsClosed)
            {
                throw new KeyStoreException("Invalid key storage provider handle.");
            }

            if (change is KeyEntry entry && entry.Variant != null && entry.Variant.KeyContainers.Count > 0)
            {
                ImportKeyValue(entry.Identifier, entry.Variant.KeyContainers[0].Key.GetAggregatedValueAsBinary());
            }
            else if (change is KeyEntryCryptogram cryptogram)
            {
                var wrappingKey = cryptogram.WrappingKey;
                if (wrappingKey == null)
                {
                    wrappingKey = Options?.WrappingKey;
                }

                SafeNCRYPT_KEY_HANDLE phWrappingKey = SafeNCRYPT_KEY_HANDLE.Null;
                if (wrappingKey != null && wrappingKey.KeyId.IsConfigured())
                {
                    if (!await CheckKeyEntryExists(wrappingKey.KeyId, change.KClass, out phWrappingKey))
                    {
                        log.Error(string.Format("The key entry `{0}` doesn't exist.", wrappingKey.KeyId));
                        throw new KeyStoreException("The key entry doesn't exist.");
                    }
                }

                if (string.IsNullOrEmpty(cryptogram.Value))
                {
                    throw new KeyStoreException("The cryptogram value is empty.");
                }

                _NCryptImportKey(change.Identifier.Id, phWrappingKey, "CipherKeyBlob", Convert.FromHexString(cryptogram.Value));
            }

            log.Info(string.Format("Key entry `{0}` created.", change.Identifier));
        }

        private void ImportKeyValue(KeyEntryId entryId, byte[]? key)
        {
            if (!string.IsNullOrEmpty(entryId.Id) && key != null)
            {
                byte[] blob = new byte[s_cipherKeyBlobPrefix.Length + key.Length];
                Buffer.BlockCopy(s_cipherKeyBlobPrefix, 0, blob, 0, s_cipherKeyBlobPrefix.Length);
                blob[12] = (byte)(12 /* sizeof(BCRYPT_KEY_DATA_BLOB_HEADER) */ + key.Length);
                blob[32] = (byte)key.Length;
                Buffer.BlockCopy(key, 0, blob, s_cipherKeyBlobPrefix.Length, key.Length);

                _NCryptImportKey(entryId.Id, IntPtr.Zero, "CipherKeyBlob", blob);
            }
        }

        private unsafe void _NCryptImportKey(string keyName, NCRYPT_KEY_HANDLE hImportKey, string pszBlobType, byte[] blob)
        {
            var nameBuf = new NCryptBuffer(KeyDerivationBufferType.NCRYPTBUFFER_PKCS_KEY_NAME, keyName);
            var bufferDesc = new NCryptBufferDesc(nameBuf);

            fixed (byte* blobPtr = blob)
            {
                var r = NCryptImportKey(_phProvider, hImportKey, pszBlobType, bufferDesc, out SafeNCRYPT_KEY_HANDLE hKey, (IntPtr)blobPtr, (uint)blob.Length);
                if (r != HRESULT.S_OK)
                {
                    throw new KeyStoreException(string.Format("NCryptImportKey failed with code: {0}", r));
                }
            }
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
                keyEntry.Variant = keyEntry.GetAllVariants(keClass).FirstOrDefault(v => v.Name == "AES");
            }
            else
            {
                log.Error(string.Format("The key store doesn't support key entry generation without specifing the target type for class `{0}`.", keClass));
                throw new NotImplementedException();
            }
            return Generate(keyEntry);
        }

        public override Task<KeyEntryId> Generate(KeyEntry entry)
        {
            if (_phProvider.IsInvalid || _phProvider.IsClosed)
            {
                throw new KeyStoreException("Invalid key storage provider handle.");
            }

            if (entry.Variant == null)
            {
                throw new KeyStoreException("A variant is required.");
            }

            SafeNCRYPT_KEY_HANDLE phKey;
            var r = NCryptCreatePersistedKey(_phProvider, out phKey, entry.Variant.Name, entry.Identifier.Id, 0, GetCreateKeyFlagScope());
            if (r != HRESULT.S_OK)
            {
                throw new KeyStoreException(string.Format("NCryptCreatePersistedKey failed with code: {0}", r));
            }

            /*int keyUsage = 0;
            var v = BitConverter.GetBytes(keyUsage);
            r = NCryptSetProperty(phKey, PropertyName.NCRYPT_KEY_USAGE_PROPERTY, v, (uint)v.Length, SetPropFlags.NCRYPT_PERSIST_FLAG | SetPropFlags.NCRYPT_SILENT_FLAG);
            if (r != HRESULT.S_OK)
            {
                log.Error(string.Format("NCryptSetProperty for property `{0}` failed with code: {1}", PropertyName.NCRYPT_VERSION_PROPERTY, r));
            }*/

            r = NCryptFinalizeKey(phKey);
            if (r != HRESULT.S_OK)
            {
                throw new KeyStoreException(string.Format("NCryptFinalizeKey failed with code: {0}", r));
            }

            log.Info(string.Format("Key entry `{0}` generated.", entry.Identifier));
            return Task.FromResult(entry.Identifier);
        }

        public override async Task Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing)
        {
            log.Info(string.Format("Deleting key entry `{0}`...", identifier));
            SafeNCRYPT_KEY_HANDLE phKey;
            var exists = await CheckKeyEntryExists(identifier, keClass, out phKey);
            if (!exists && !ignoreIfMissing)
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", identifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            if (exists)
            {
                try
                {
                    var r = NCryptDeleteKey(phKey);
                    if (r != HRESULT.S_OK)
                    {
                        log.Error(string.Format("NCryptDeleteKey failed with code: {0}", r));
                        throw new KeyStoreException(string.Format("NCryptDeleteKey failed with code: {0}", r));
                    }

                    log.Info(string.Format("Key entry `{0}` deleted.", identifier));
                }
                finally
                {
                    NCryptFreeObject(phKey.DangerousGetHandle());
                }
            }
        }



        private unsafe byte[]? _NCryptGetProperty(SafeNCRYPT_KEY_HANDLE phKey, string propertyName)
        {
            uint pcbResult = 0;
            var r = NCryptGetProperty(phKey, propertyName, 0, 0, out pcbResult, GetPropertyFlags.NCRYPT_SILENT_FLAG);
            if (r != HRESULT.S_OK)
            {
                log.Error(string.Format("NCryptGetProperty for `{0}` failed with code: {1}", propertyName, r));
            }

            if (pcbResult > 0)
            {
                var output = new byte[pcbResult];
                fixed (byte* pbOutput = output)
                {
                    r = NCryptGetProperty(phKey, propertyName, (IntPtr)pbOutput, pcbResult, out pcbResult, GetPropertyFlags.NCRYPT_SILENT_FLAG);
                    if (r != HRESULT.S_OK)
                    {
                        log.Error(string.Format("NCryptGetProperty for `{0}` failed with code: {1}", propertyName, r));
                    }
                    return output;
                }
            }

            return null;
        }

        public override async Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            log.Info(string.Format("Getting key entry `{0}`...", identifier));
            CNGKeyEntry? ke = null;

            SafeNCRYPT_KEY_HANDLE phKey;
            if (!await CheckKeyEntryExists(identifier, keClass, out phKey))
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", identifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            try
            {
                var rawalgo = _NCryptGetProperty(phKey, PropertyName.NCRYPT_ALGORITHM_PROPERTY);
                if (rawalgo == null || rawalgo.Length < 2)
                {
                    throw new KeyStoreException(string.Format("Cannot retrieve the algorithm for key `{0}`.", identifier.Id));
                }
                var algo = Encoding.Unicode.GetString(rawalgo, 0, rawalgo.Length - 2).ToUpperInvariant();
                ke = new CNGKeyEntry(keClass);
                ke.Identifier = identifier;
                ke.SetVariant(algo);

                var v = _NCryptGetProperty(phKey, PropertyName.NCRYPT_KEY_USAGE_PROPERTY);
                if (v != null && v.Length > 0)
                {
                    var usages = BitConverter.ToInt32(v);
                }
            }
            finally
            {
                NCryptFreeObject(phKey.DangerousGetHandle());
            }

            return ke;
        }

        public override async Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass)
        {
            log.Info(string.Format("Getting all key entries (class: `{0}`)...", keClass));
            var entries = new List<KeyEntryId>();

            if (_phProvider.IsInvalid || _phProvider.IsClosed)
            {
                throw new KeyStoreException("Invalid key storage provider handle.");
            }

            nint ppEnumState = 0;
            SafeNCryptBuffer ppKeyName;
            HRESULT r = HRESULT.S_OK;
            do
            {
                r = NCryptEnumKeys(_phProvider, null, out ppKeyName, ref ppEnumState, GetOpenKeyFlagScope());
                if (r == HRESULT.S_OK)
                {
                    var keyName = ppKeyName.ToStructure<NCryptKeyName>();
                    var keyClass = CNGKeyEntry.GetKeyEntryClassFromAlgId(keyName.pszAlgId);
                    if (keClass == null || keClass == keyClass)
                    {
                        entries.Add(new KeyEntryId
                        {
                            Id = keyName.pszName
                        });
                    }
                }
                else
                {
                    if (r != HRESULT.NTE_NO_MORE_ITEMS)
                    {
                        throw new KeyStoreException(string.Format("NCryptEnumKeys failed with code: {0}", r));
                    }
                }
            } while (r == HRESULT.S_OK);

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

            throw new NotSupportedException();
        }

        public override KeyEntry? GetDefaultKeyEntry(KeyEntryClass keClass)
        {
            return base.GetDefaultKeyEntry(keClass) ?? new CNGKeyEntry(keClass);
        }
#pragma warning restore CA1416 // Validate platform compatibility
    }
}
