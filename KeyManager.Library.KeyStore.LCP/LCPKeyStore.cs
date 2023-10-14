using Leosac.CredentialProvisioning.API;
using Leosac.CredentialProvisioning.Server.Contracts.Models;
using Refit;
using Leosac.CredentialProvisioning.Server.Contracts;
using Leosac.CredentialProvisioning.Core.Models;

namespace Leosac.KeyManager.Library.KeyStore.LCP
{
    public class LCPKeyStore : KeyStore
    {
        const string PLACEHOLDER = "Key Store Placeholder";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private IAuthenticationAPI? _authAPI;
        private ICredentialKeyAPI? _keyAPI;
        private string? _authToken;

        public LCPKeyStore()
        {

        }

        public override string Name => "LCP";

        public override bool CanCreateKeyEntries => true;

        public override bool CanDeleteKeyEntries => true;

        public override bool CanReorderKeyEntries => false;

        public override IEnumerable<KeyEntryClass> SupportedClasses
        {
            get => new KeyEntryClass[] { KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric };
        }

        public override async Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            CheckAuthentication();

            if (!string.IsNullOrEmpty(identifier.Id))
            {
                var key = await _keyAPI!.Get(Guid.Parse(identifier.Id));
                return (key != null);
            }

            return false;
        }

        public override Task Close()
        {
            log.Info("Closing the key store...");

            _authToken = null;

            log.Info("Key Store closed.");
            return Task.CompletedTask;
        }

        public override async Task Create(IChangeKeyEntry change)
        {
            log.Info(string.Format("Creating key entry `{0}`...", change.Identifier));
            CheckAuthentication();

            if (change is KeyEntry entry && entry.Variant != null && entry.Variant.KeyContainers.Count > 0)
            {
                var key = CreateCredentialKey(entry.Identifier, entry.Variant.KeyContainers[0] as KeyVersion, (entry as LCPKeyEntry)?.LCPProperties);
                if (key != null)
                {
                    await _keyAPI!.Create(key);
                }
            }
            else if (change is KeyEntryCryptogram cryptogram)
            {
                if (cryptogram.WrappingKeyId == null)
                {
                    log.Error("Wrapping Key Entry Identifier parameter is expected.");
                    throw new KeyStoreException("Wrapping Key Entry Identifier parameter is expected.");
                }

                if (!await CheckKeyEntryExists(cryptogram.WrappingKeyId, change.KClass))
                {
                    log.Error(string.Format("The key entry `{0}` doesn't exist.", cryptogram.WrappingKeyId));
                    throw new KeyStoreException("The key entry doesn't exist.");
                }

                throw new NotSupportedException();
            }

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
                await _keyAPI!.Delete(Guid.Parse(identifier.Id!));
                log.Info(string.Format("Key entry `{0}` deleted.", identifier));
            }
        }

        public override async Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            log.Info(string.Format("Getting key entry `{0}`...", identifier));
            CheckAuthentication();

            LCPKeyEntry? ke = null;

            if (!string.IsNullOrEmpty(identifier.Id))
            {
                var key = await _keyAPI!.Get(Guid.Parse(identifier.Id));
                if (key != null)
                {
                    ke = new LCPKeyEntry
                    {
                        Identifier = new KeyEntryId
                        {
                            Id = key.Id.ToString(),
                            Label = key.Name
                        }
                    };
                    ke.Variant = ke.CreateVariantFromKeyType(key.KeyType);
                    if (ke.Variant != null && ke.Variant.KeyContainers.Count > 0)
                    {
                        var kc = ke.Variant.KeyContainers[0];
                        if (!string.IsNullOrEmpty(key.Value))
                        {
                            kc.Key.SetAggregatedValue(key.Value);
                        }
                        if (kc is KeyVersion kv && key.Version != null)
                        {
                            kv.Version = key.Version.Value;
                        }

                        if (key.KeyStoreType == "database")
                        {
                            kc.Key.Link.KeyStoreFavorite = null;
                        }
                        else
                        {
                            kc.Key.Link.KeyStoreFavorite = key.KeyStore ?? PLACEHOLDER;
                            kc.Key.Link.KeyIdentifier.Id = key.KeyStoreReference;
                        }
                    }
                    ke.LCPProperties!.Scope = key.Scope;
                    ke.LCPProperties.ScopeDiversifier = key.ScopeDiversifier;
                    log.Info(string.Format("Key entry `{0}` retrieved.", identifier));
                }
            }

            return ke;
        }

        public override async Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass = null)
        {
            log.Info(string.Format("Getting all key entries (class: `{0}`)...", keClass));

            CheckAuthentication();
            var entries = new List<KeyEntryId>();

            var keys = await _keyAPI!.GetAll();
            foreach (var k in keys)
            {
                entries.Add(new KeyEntryId()
                {
                    Id = k.Id.ToString(),
                    Label = k.Name
                });
            }

            log.Info(string.Format("{0} key entries returned.", entries.Count));
            return entries;
        }

        private void CheckAuthentication()
        {
            if (_authAPI == null || _keyAPI == null || string.IsNullOrEmpty(_authToken))
                throw new Exception("Please open the key store first to perform authentication.");
        }

        public LCPKeyStoreProperties GetLCPProperties()
        {
            var p = Properties as LCPKeyStoreProperties;
            return p ?? throw new KeyStoreException("Missing LCP key store properties.");
        }

        public override Task MoveDown(KeyEntryId identifier, KeyEntryClass keClass)
        {
            throw new NotImplementedException();
        }

        public override Task MoveUp(KeyEntryId identifier, KeyEntryClass keClass)
        {
            throw new NotImplementedException();
        }

        public override async Task Open()
        {
            log.Info("Opening the key store...");

            var p = GetLCPProperties();
            var apiRoot = string.Format("{0}/api", p.ServerAddress);
            _authAPI = RestService.For<IAuthenticationAPI>(apiRoot);
            var authr = await _authAPI.Authenticate(new AuthenticateWithUserPwdRequest
            {
                UserName = p.Username,
                Password = p.Secret ?? string.Empty
            });
            _authToken = authr.TokenValue;

            var refitSettings = new RefitSettings(new SystemTextJsonContentSerializer(LCPSerializationHelper.CreateSerializerOptions()))
            {
                AuthorizationHeaderValueGetter = (request, cancel) =>
                    Task.FromResult(_authToken ?? string.Empty)
            };
            _keyAPI = RestService.For<ICredentialKeyAPI>(string.Format("{0}/CredentialKey", apiRoot), refitSettings);

            log.Info("Key Store opened.");
        }

        public override Task<string?> ResolveKeyEntryLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? divInput = null, KeyEntryId? wrappingKeyId = null, string? wrappingContainerSelector = null)
        {
            throw new NotSupportedException();
        }

        public override Task<string?> ResolveKeyLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? containerSelector = null, string? divInput = null)
        {
            throw new NotSupportedException();
        }

        public override async Task Store(IList<IChangeKeyEntry> changes)
        {
            log.Info(string.Format("Storing `{0}` key entries...", changes.Count));

            foreach (var change in changes)
            {
                if (await CheckKeyEntryExists(change.Identifier, change.KClass))
                {
                    await Update(change);
                }
                else
                {
                    await Create(change);
                }
            }

            log.Info("Key Entries storing completed.");
        }

        public override async Task Update(IChangeKeyEntry change, bool ignoreIfMissing = false)
        {
            log.Info(string.Format("Updating key entry `{0}`...", change.Identifier));

            if (!await CheckKeyEntryExists(change.Identifier, change.KClass))
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", change.Identifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            if (change is KeyEntry entry)
            {
                if (entry.Variant?.KeyContainers.Count == 1)
                {
                    var key = CreateCredentialKey(entry.Identifier, entry.Variant.KeyContainers[0] as KeyVersion, (entry as LCPKeyEntry)?.LCPProperties);
                    if (key != null)
                    {
                        // We should already have only one key material during an update
                        await _keyAPI!.Update(key.Id, key);
                    }
                }
            }
            else
                throw new NotImplementedException();

            log.Info(string.Format("Key entry `{0}` updated.", change.Identifier));
        }

        private static CredentialKey CreateCredentialKey(KeyEntryId identifier, KeyContainer? kc, LCPKeyEntryProperties? properties)
        {
            var key = new CredentialKey();
            if (!string.IsNullOrEmpty(identifier.Id))
            {
                key.Id = Guid.Parse(identifier.Id);
            }
            if (kc != null)
            {
                var rawkey = kc.Key.GetAggregatedValue<byte[]>(KeyValueFormat.Binary);
                key.Value = (rawkey != null) ? Convert.ToHexString(rawkey) : null;
            }
            if (!string.IsNullOrEmpty(kc?.Key.Link?.KeyStoreFavorite))
            {
                key.KeyStore = kc.Key.Link.KeyStoreFavorite != PLACEHOLDER ? kc.Key.Link.KeyStoreFavorite : null;
                key.KeyStoreReference = kc.Key.Link.KeyIdentifier?.Id;
                key.KeyStoreType = "sam"; // TODO: get the referenced key store type here
            }
            else
            {
                key.KeyStoreType = "database";
            }

            if (kc is KeyVersion kv)
            {
                key.Version = kv.Version;
            }
            if (properties != null)
            {
                key.Scope = properties.Scope;
                key.ScopeDiversifier = properties.ScopeDiversifier;
            }
            return key;
        }
    }
}
