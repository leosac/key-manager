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

        private static byte[] s_cipherKeyBlobPrefixP1 = {
            // NCRYPT_KEY_BLOB_HEADER.cbSize (16)
            0x10, 0x00, 0x00, 0x00,
            // NCRYPT_KEY_BLOB_HEADER.dwMagic (NCRYPT_CIPHER_KEY_BLOB_MAGIC (0x52485043))
            0x43, 0x50, 0x48, 0x52,
            // NCRYPT_KEY_BLOB_HEADER.cbAlgName (to be determined)
            0x00, 0x00, 0x00, 0x00,
            // NCRYPT_KEY_BLOB_HEADER.cbKeyData (to be determined)
            0x00, 0x00, 0x00, 0x00
        };

        private static byte[] s_cipherKeyBlobPrefixP2 = {
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
                ImportKeyValue(entry.Identifier, entry.Variant.Name, entry.Variant.KeyContainers[0].Key.GetAggregatedValueAsBinary(), entry.Properties as CNGKeyEntryProperties);
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

                _NCryptImportKey(change.Identifier.Id, phWrappingKey, "CipherKeyBlob", Convert.FromHexString(cryptogram.Value), null);
            }

            log.Info(string.Format("Key entry `{0}` created.", change.Identifier));
        }

        private void ImportKeyValue(KeyEntryId entryId, string? algoId, byte[]? key, CNGKeyEntryProperties? properties)
        {
            if (!string.IsNullOrEmpty(entryId.Id) && key != null)
            {
                if (string.IsNullOrEmpty(algoId))
                {
                    algoId = "AES";
                }
                algoId += "\0";
                var algoBlob = Encoding.Unicode.GetBytes(algoId);
                byte[] blob = new byte[s_cipherKeyBlobPrefixP1.Length + algoBlob.Length + s_cipherKeyBlobPrefixP2.Length + key.Length];
                Buffer.BlockCopy(s_cipherKeyBlobPrefixP1, 0, blob, 0, s_cipherKeyBlobPrefixP1.Length);
                blob[8] = (byte)algoBlob.Length;
                blob[12] = (byte)(12 /* sizeof(BCRYPT_KEY_DATA_BLOB_HEADER) */ + key.Length);
                Buffer.BlockCopy(algoBlob, 0, blob, s_cipherKeyBlobPrefixP1.Length, algoBlob.Length);
                Buffer.BlockCopy(s_cipherKeyBlobPrefixP2, 0, blob, s_cipherKeyBlobPrefixP1.Length + algoBlob.Length, s_cipherKeyBlobPrefixP2.Length);
                blob[24 + blob[8]] = (byte)key.Length;
                Buffer.BlockCopy(key, 0, blob, s_cipherKeyBlobPrefixP1.Length + algoBlob.Length + s_cipherKeyBlobPrefixP2.Length, key.Length);

                _NCryptImportKey(entryId.Id, IntPtr.Zero, "CipherKeyBlob", blob, properties);
            }
        }

        private unsafe void _NCryptImportKey(string keyName, NCRYPT_KEY_HANDLE hImportKey, string pszBlobType, byte[] blob, CNGKeyEntryProperties? properties)
        {
            var nameBuf = new NCryptBuffer(KeyDerivationBufferType.NCRYPTBUFFER_PKCS_KEY_NAME, keyName);
            var bufferDesc = new NCryptBufferDesc(nameBuf);

            fixed (byte* blobPtr = blob)
            {
                var r = NCryptImportKey(_phProvider, hImportKey, pszBlobType, bufferDesc, out SafeNCRYPT_KEY_HANDLE hKey, (IntPtr)blobPtr, (uint)blob.Length, /* NCRYPT_DO_NOT_FINALIZE_FLAG */ (NCryptUIFlags)0x400);
                if (r != HRESULT.S_OK)
                {
                    throw new KeyStoreException(string.Format("NCryptImportKey failed with code: {0}", r));
                }
                SetKeyPropertiesAndFinalize(hKey, properties);
            }
        }

        private void SetKeyPropertiesAndFinalize(SafeNCRYPT_KEY_HANDLE phKey, CNGKeyEntryProperties? properties)
        {
            HRESULT r;
            if (properties != null)
            {
                var raw = GetRawProperties(properties);
                foreach (var p in raw.Keys)
                {
                    r = NCryptSetProperty(phKey, p, raw[p], (uint)raw[p].Length, SetPropFlags.NCRYPT_PERSIST_FLAG | SetPropFlags.NCRYPT_SILENT_FLAG);
                    if (r != HRESULT.S_OK)
                    {
                        log.Error(string.Format("NCryptSetProperty for property `{0}` failed with code: {1}", p, r));
                    }
                }
            }

            r = NCryptFinalizeKey(phKey);
            if (r != HRESULT.S_OK)
            {
                throw new KeyStoreException(string.Format("NCryptFinalizeKey failed with code: {0}", r));
            }
        }

        protected IDictionary<string, byte[]> GetRawProperties(CNGKeyEntryProperties properties)
        {
            var raw = new Dictionary<string, byte[]>();
            int keyUsage = (properties.UsageAllowAll ? (int)KeyUsage.NCRYPT_ALLOW_ALL_USAGES : 0) |
                               (properties.UsageAllowDecrypt ? (int)KeyUsage.NCRYPT_ALLOW_DECRYPT_FLAG : 0) |
                               (properties.UsageAllowSigning ? (int)KeyUsage.NCRYPT_ALLOW_SIGNING_FLAG : 0) |
                               (properties.UsageAllowKeyAgreement ? (int)KeyUsage.NCRYPT_ALLOW_KEY_AGREEMENT_FLAG : 0) |
                               (properties.UsageAllowKeyAttestation ? 0x10 /* NCRYPT_ALLOW_KEY_ATTESTATION_FLAG */ : 0);
            var v = BitConverter.GetBytes(keyUsage);
            raw.Add(PropertyName.NCRYPT_KEY_USAGE_PROPERTY, v);

            int exportPolicy = (properties.ExportAllowExport ? (int)ExportPolicy.NCRYPT_ALLOW_EXPORT_FLAG : 0) |
                               (properties.ExportAllowPlainExport ? (int)ExportPolicy.NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG : 0) |
                               (properties.ExportAllowArchiving ? (int)ExportPolicy.NCRYPT_ALLOW_ARCHIVING_FLAG : 0) |
                               (properties.ExportAllowPlainArchiving ? (int)ExportPolicy.NCRYPT_ALLOW_PLAINTEXT_ARCHIVING_FLAG : 0);
            v = BitConverter.GetBytes(exportPolicy);
            raw.Add(PropertyName.NCRYPT_EXPORT_POLICY_PROPERTY, v);

            return raw;
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

            SetKeyPropertiesAndFinalize(phKey, entry.Properties as CNGKeyEntryProperties);

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

                if (ke.CNGProperties != null)
                {
                    var v = _NCryptGetProperty(phKey, PropertyName.NCRYPT_KEY_USAGE_PROPERTY);
                    if (v != null && v.Length > 0)
                    {
                        var usages = BitConverter.ToInt32(v);
                        ke.CNGProperties.UsageAllowAll = (usages == (int)KeyUsage.NCRYPT_ALLOW_ALL_USAGES);
                        ke.CNGProperties.UsageAllowDecrypt = ((usages & (int)KeyUsage.NCRYPT_ALLOW_DECRYPT_FLAG) == (int)KeyUsage.NCRYPT_ALLOW_DECRYPT_FLAG);
                        ke.CNGProperties.UsageAllowSigning = ((usages & (int)KeyUsage.NCRYPT_ALLOW_SIGNING_FLAG) == (int)KeyUsage.NCRYPT_ALLOW_SIGNING_FLAG);
                        ke.CNGProperties.UsageAllowKeyAgreement = ((usages & (int)KeyUsage.NCRYPT_ALLOW_KEY_AGREEMENT_FLAG) == (int)KeyUsage.NCRYPT_ALLOW_KEY_AGREEMENT_FLAG);
                        ke.CNGProperties.UsageAllowKeyAttestation = ((usages & 0x10 /* NCRYPT_ALLOW_KEY_ATTESTATION */) == 0x10);
                    }
                    v = _NCryptGetProperty(phKey, PropertyName.NCRYPT_EXPORT_POLICY_PROPERTY);
                    if (v != null && v.Length > 0)
                    {
                        var exportPolicy = BitConverter.ToInt32(v);
                        ke.CNGProperties.ExportAllowExport = ((exportPolicy & (int)ExportPolicy.NCRYPT_ALLOW_EXPORT_FLAG) == (int)ExportPolicy.NCRYPT_ALLOW_EXPORT_FLAG);
                        ke.CNGProperties.ExportAllowPlainExport = ((exportPolicy & (int)ExportPolicy.NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG) == (int)ExportPolicy.NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG);
                        ke.CNGProperties.ExportAllowArchiving = ((exportPolicy & (int)ExportPolicy.NCRYPT_ALLOW_ARCHIVING_FLAG) == (int)ExportPolicy.NCRYPT_ALLOW_ARCHIVING_FLAG);
                        ke.CNGProperties.ExportAllowPlainArchiving = ((exportPolicy & (int)ExportPolicy.NCRYPT_ALLOW_PLAINTEXT_ARCHIVING_FLAG) == (int)ExportPolicy.NCRYPT_ALLOW_PLAINTEXT_ARCHIVING_FLAG);
                    }
                }
            }
            finally
            {
                NCryptFreeObject(phKey.DangerousGetHandle());
            }

            return ke;
        }

        protected async Task<byte[]> _ResolveKeyEntryLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, WrappingKey? wrappingKey)
        {
            SafeNCRYPT_KEY_HANDLE phKey;
            if (!await CheckKeyEntryExists(keyIdentifier, keClass, out phKey))
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", keyIdentifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            SafeNCRYPT_KEY_HANDLE? phExportKey = null;
            if (wrappingKey != null && wrappingKey.KeyId.IsConfigured())
            {
                if (!await CheckKeyEntryExists(wrappingKey.KeyId, keClass, out phExportKey))
                {
                    log.Error(string.Format("The key entry `{0}` doesn't exist.", keyIdentifier));
                    throw new KeyStoreException("The key entry doesn't exist.");
                }
            }

            SafeAllocatedMemoryHandle pbOutput;
            HRESULT r = NCryptExportKey(phKey, phExportKey != null ? phExportKey : IntPtr.Zero, "CipherKeyBlob", out pbOutput);
            if (r != HRESULT.S_OK)
            {
                throw new KeyStoreException(string.Format("NCryptExportKey failed with code: {0}", r));
            }

            return pbOutput.GetBytes();
        }

        public override async Task<string?> ResolveKeyEntryLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? divInput, WrappingKey? wrappingKey, KeyEntryId? targetKeyIdentifier)
        {
            log.Info(string.Format("Resolving key entry link with Key Entry Identifier `{0}` and Wrapping Key Entry Identifier `{1}`...", keyIdentifier, wrappingKey?.KeyId));
            if (!string.IsNullOrEmpty(divInput))
            {
                throw new KeyStoreException("Div input is not supported by the CNG key store.");
            }

            return Convert.ToHexString(await _ResolveKeyEntryLink(keyIdentifier, keClass, wrappingKey));
        }

        public override async Task<string?> ResolveKeyLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? containerSelector, string? divInput)
        {
            log.Info(string.Format("Resolving key link with Key Entry Identifier `{0}`, Key Version `{1}`, Div Input `{2}`...", keyIdentifier, containerSelector, divInput));
            if (!string.IsNullOrEmpty(divInput))
            {
                throw new KeyStoreException("Div input is not supported by the CNG key store.");
            }

            var keLink = await _ResolveKeyEntryLink(keyIdentifier, keClass, null);
            if (keLink.Length < 12)
            {
                throw new KeyStoreException("Unexpected key buffer length.");
            }
            var algolen = (keLink[11] << 24) | (keLink[10] << 16) | (keLink[9] << 8) | keLink[8];
            if (keLink.Length < 28 + algolen)
            {
                throw new KeyStoreException("Unexpected key buffer length.");
            }
            var keylen = (keLink[27 + algolen] << 24) | (keLink[26 + algolen] << 16) | (keLink[25 + algolen] << 8) | keLink[24 + algolen];
            var p = s_cipherKeyBlobPrefixP1.Length + algolen + s_cipherKeyBlobPrefixP2.Length;
            if (keLink.Length < p + keylen)
            {
                throw new KeyStoreException("Unexpected key buffer length.");
            }
            return Convert.ToHexString(keLink[p..(p+keylen)]);
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
