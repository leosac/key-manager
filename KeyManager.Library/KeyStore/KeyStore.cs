using Leosac.KeyManager.Library.DivInput;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore
{
    /// <summary>
    /// The base class for a Key Store implementation.
    /// </summary>
    public abstract class KeyStore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        protected readonly JsonSerializerSettings _jsonSettings;

        protected KeyStore()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                Formatting = Formatting.Indented
            };
        }

        /// <summary>
        /// The key store name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// True to allow key entry creation, false otherwise.
        /// </summary>
        public abstract bool CanCreateKeyEntries { get; }

        /// <summary>
        /// True to allow key entry deletion, false otherwise.
        /// </summary>
        public abstract bool CanDeleteKeyEntries { get; }

        /// <summary>
        /// True to allow key entry update, false otherwise.
        /// </summary>
        public virtual bool CanUpdateKeyEntries => true;

        /// <summary>
        /// True if key entries can be reordered, false otherwise.
        /// </summary>
        public virtual bool CanReorderKeyEntries => false;

        /// <summary>
        /// Get the supported key entry classes.
        /// </summary>
        public abstract IEnumerable<KeyEntryClass> SupportedClasses { get; }

        /// <summary>
        /// The key store properties.
        /// </summary>
        public KeyStoreProperties? Properties { get; set; }

        public bool CreateIfMissing { get; set; }

        public Task<bool> CheckKeyEntryExists(KeyEntry keyEntry)
        {
            return CheckKeyEntryExists(keyEntry.Identifier, keyEntry.KClass);
        }

        /// <summary>
        /// Open the key store.
        /// </summary>
        public abstract Task Open();

        /// <summary>
        /// Close the key store.
        /// </summary>
        public abstract Task Close();

        /// <summary>
        /// Check if a key entry exists.
        /// </summary>
        /// <param name="identifier">The key entry identifier</param>
        /// <param name="keClass">The key entry class</param>
        /// <returns></returns>
        public abstract Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass);

        /// <summary>
        /// Get all key entry identifiers.
        /// </summary>
        /// <returns>List of key entry identifiers</returns>
        public Task<IList<KeyEntryId>> GetAll()
        {
            return GetAll(null);
        }

        /// <summary>
        /// Get all key entry identifiers.
        /// </summary>
        /// <param name="keClass">The key entry class (optional, null means all)</param>
        /// <returns>List of key entry identifiers</returns>
        public abstract Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass);

        /// <summary>
        /// Create a new key entry.
        /// </summary>
        /// <param name="keyEntry">The key entry details</param>
        public abstract Task Create(IChangeKeyEntry keyEntry);

        /// <summary>
        /// Get a key entry.
        /// </summary>
        /// <param name="identifier">The key entry identifier</param>
        /// <param name="keClass">The key entry class</param>
        /// <returns>The key entry</returns>
        public abstract Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass);

        /// <summary>
        /// Update an existing key entry.
        /// </summary>
        /// <param name="keyEntry">The key entry details</param>
        /// <param name="ignoreIfMissing">Ignore if the targeted key entry is missing, throw otherwise.</param>
        public abstract Task Update(IChangeKeyEntry keyEntry, bool ignoreIfMissing);

        /// <summary>
        /// Update an existing key entry.
        /// </summary>
        /// <param name="keyEntry">The key entry details</param>
        public Task Update(IChangeKeyEntry keyEntry)
        {
            return Update(keyEntry, false);
        }

        /// <summary>
        /// Delete an existing key entry.
        /// </summary>
        /// <param name="identifier">The key entry identifier</param>
        /// <param name="keClass">The key entry class</param>
        /// <param name="ignoreIfMissing">Ignore if the targeted key entry is missing, throw otherwise.</param>
        public abstract Task Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing);

        /// <summary>
        /// Delete an existing key entry.
        /// </summary>
        /// <param name="identifier">The key entry identifier</param>
        /// <param name="keClass">The key entry class</param>
        public Task Delete(KeyEntryId identifier, KeyEntryClass keClass)
        {
            return Delete(identifier, keClass, false);
        }

        /// <summary>
        /// Move up a key entry on the list, if reordering is supported.
        /// </summary>
        /// <param name="identifier">The key entry identifier</param>
        /// <param name="keClass">The key entry class</param>
        public virtual Task MoveUp(KeyEntryId identifier, KeyEntryClass keClass)
        {
            log.Info(string.Format("Moving Up key entry `{0}` of class `{1}`...", identifier, keClass));
            log.Error("The key store doesn't support key entries reordering.");
            throw new KeyStoreException("The key store doesn't support key entries reordering.");
        }

        /// <summary>
        /// Move down a key entry on the list, if reordering is supported.
        /// </summary>
        /// <param name="identifier">The key entry identifier</param>
        /// <param name="keClass">The key entry class</param>
        public virtual Task MoveDown(KeyEntryId identifier, KeyEntryClass keClass)
        {
            log.Info(string.Format("Moving Down key entry `{0}` of class `{1}`...", identifier, keClass));
            log.Error("The key store doesn't support key entries reordering.");
            throw new KeyStoreException("The key store doesn't support key entries reordering.");
        }

        /// <summary>
        /// Store a key entry change.
        /// </summary>
        /// <param name="change">The key entry details.</param>
        public virtual Task Store(IChangeKeyEntry change)
        {
            return Store(new List<IChangeKeyEntry>
            {
                change
            });
        }

        /// <summary>
        /// Store a list of key entry changes.
        /// </summary>
        /// <param name="changes">The key entries details.</param>
        public abstract Task Store(IList<IChangeKeyEntry> changes);

        public virtual async Task Publish(KeyStore store, Func<string, KeyStore?> getFavoriteKeyStore, KeyEntryId? wrappingKeyId, string? wrappingContainerSelector, Action<KeyStore, KeyEntryClass, int>? initCallback)
        {
            var classes = SupportedClasses;
            foreach (var keClass in classes)
            {
                await Publish(store, getFavoriteKeyStore, keClass, wrappingKeyId, wrappingContainerSelector, initCallback);
            }
        }

        public virtual async Task Publish(KeyStore store, Func<string, KeyStore?> getFavoriteKeyStore, KeyEntryClass keClass, KeyEntryId? wrappingKeyId, string? wrappingContainerSelector, Action<KeyStore, KeyEntryClass, int>? initCallback)
        {
            var ids = await GetAll(keClass);
            await Publish(store, getFavoriteKeyStore, ids, keClass, wrappingKeyId, wrappingContainerSelector, initCallback);
        }

        public virtual async Task Publish(KeyStore store, Func<string, KeyStore?> getFavoriteKeyStore, IEnumerable<KeyEntryId> ids, KeyEntryClass keClass, KeyEntryId? wrappingKeyId, string? wrappingContainerSelector, Action<KeyStore, KeyEntryClass, int>? initCallback)
        {
            var changes = new List<IChangeKeyEntry>();
            initCallback?.Invoke(this, keClass, ids.Count());
            foreach (var id in ids)
            {
                var entry = await Get(id, keClass);
                if (entry != null)
                {
                    if (entry.Link != null && entry.Link.KeyIdentifier.IsConfigured() && !string.IsNullOrEmpty(entry.Link.KeyStoreFavorite))
                    {
                        var cryptogram = new KeyEntryCryptogram
                        {
                            Identifier = id,
                            // TODO: we may want to have multiple wrapping keys later on
                            WrappingKeyId = wrappingKeyId,
                            WrappingContainerSelector = wrappingContainerSelector
                        };

                        var ks = getFavoriteKeyStore(entry.Link.KeyStoreFavorite);
                        if (ks != null)
                        {
                            await ks.Open();
                            var divContext = new DivInput.DivInputContext
                            {
                                KeyStore = ks,
                                KeyEntry = entry
                            };
                            cryptogram.Value = await ks.ResolveKeyEntryLink(entry.Link.KeyIdentifier, keClass, ComputeDivInput(divContext, entry.Link.DivInput), entry.Link.WrappingKeyId, entry.Link.WrappingKeySelector);
                            await ks.Close();
                        }
                    }
                    else
                    {
                        if (entry.Variant != null)
                        {
                            foreach (var kv in entry.Variant.KeyContainers)
                            {
                                if (kv.Key.Link != null && kv.Key.Link.KeyIdentifier.IsConfigured() && !string.IsNullOrEmpty(kv.Key.Link.KeyStoreFavorite))
                                {
                                    var ks = getFavoriteKeyStore(kv.Key.Link.KeyStoreFavorite);
                                    if (ks != null)
                                    {
                                        await ks.Open();
                                        var divContext = new DivInput.DivInputContext
                                        {
                                            KeyStore = ks,
                                            KeyEntry = entry,
                                            KeyContainer = kv
                                        };
                                        kv.Key.SetAggregatedValueString(await ks.ResolveKeyLink(kv.Key.Link.KeyIdentifier, keClass, kv.Key.Link.ContainerSelector, ComputeDivInput(divContext, kv.Key.Link.DivInput)));
                                        await ks.Close();
                                    }
                                }
                            }
                        }
                        changes.Add(entry);
                    }
                }
            }

            await store.Open();
            await store.Store(changes);
            await store.Close();
        }

        private static string? ComputeDivInput(DivInputContext divContext, IList<DivInputFragment> divInput)
        {
            if (divContext != null && divInput != null && divInput.Count > 0)
            {
                throw new NotImplementedException();
            }

            return null;
        }

        protected void OnKeyEntryRetrieved(KeyEntry keyEntry)
        {
            KeyEntryRetrieved?.Invoke(this, keyEntry);
        }

        protected void OnKeyEntryUpdated(IChangeKeyEntry keyEntry)
        {
            KeyEntryUpdated?.Invoke(this, keyEntry);
        }

        /// <summary>
        /// Get a key from a key entry.
        /// </summary>
        /// <param name="keyIdentifier">The key entry identifier</param>
        /// <param name="keClass">The key entry class</param>
        public Task<Key?> GetKey(KeyEntryId keyIdentifier, KeyEntryClass keClass)
        {
            return GetKey(keyIdentifier, keClass, null);
        }

        /// <summary>
        /// Get a key from a key entry.
        /// </summary>
        /// <param name="keyIdentifier">The key entry identifier</param>
        /// <param name="keClass">The key entry class</param>
        /// <param name="keyContainerSelector">The key container selector</param>
        /// <returns></returns>
        public async Task<Key?> GetKey(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? keyContainerSelector)
        {
            log.Info(String.Format("Getting key with Key Entry Identifier `{0}` and Container Selector `{1}`...", keyIdentifier, keyContainerSelector));
            var keyEntry = await Get(keyIdentifier, keClass);
            if (keyEntry != null)
            {
                if (keyEntry.Variant != null)
                {
                    KeyContainer? kv;
                    if (!string.IsNullOrEmpty(keyContainerSelector))
                    {
                        kv = keyEntry.Variant.KeyContainers.OfType<KeyVersion>().Where(kv => kv.Version.ToString() == keyContainerSelector).FirstOrDefault();
                    }
                    else
                    {
                        kv = keyEntry.Variant.KeyContainers[0];
                    }

                    if (kv != null)
                    {
                        return kv.Key;
                    }
                    else
                    {
                        log.Error(String.Format("Cannot found the key container with selector `{0}` on key entry `{1}`", keyContainerSelector, keyIdentifier));
                    }
                }
                else
                {
                    log.Error("No key variant set on the Key Entry.");
                }
            }
            else
            {
                log.Error(String.Format("Key Entry Identifier `{0}` cannot be found.", keyIdentifier));
            }

            return null;
        }

        public async Task<string?> GetKeyValue(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? keyContainerSelector, string? divInput)
        {
            if (!string.IsNullOrEmpty(divInput))
            {
                log.Error("Div Input parameter is not yet supported.");
                throw new KeyStoreException("Div Input parameter is not yet supported.");
            }

            var key = await GetKey(keyIdentifier, keClass, keyContainerSelector);
            return key?.GetAggregatedValueString();
        }

        /// <summary>
        /// Resolve a key entry link.
        /// </summary>
        /// <param name="keyIdentifier">The key entry identifier</param>
        /// <param name="keClass">The key entry class</param>
        /// <param name="divInput">The key div input (optional)</param>
        /// <param name="wrappingKeyId">The wrapping key identifier for cryptogram computation (optional)</param>
        /// <param name="wrappingKeyContainerSelector">The wrapping key container selector for cryptogram computation (optional)</param>
        /// <returns>The change key entry cryptogram</returns>
        public virtual async Task<string?> ResolveKeyEntryLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? divInput, KeyEntryId? wrappingKeyId, string? wrappingKeyContainerSelector)
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

        /// <summary>
        /// Resolve a key link.
        /// </summary>
        /// <param name="keyIdentifier">The key entry identifier</param>
        /// <param name="keClass">The key entry class</param>
        /// <param name="containerSelector">The key container selector (optional)</param>
        /// <param name="divInput">The key div input (optional)</param>
        /// <returns>The key value</returns>


        public virtual async Task<string?> ResolveKeyLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? containerSelector, string? divInput)
        {
            log.Info(string.Format("Resolving key link with Key Entry Identifier `{0}`, Container Selector `{1}`, Div Input `{2}`...", keyIdentifier, containerSelector, divInput));

            if (!await CheckKeyEntryExists(keyIdentifier, keClass))
            {
                log.Error(string.Format("The key entry `{0}` do not exists.", keyIdentifier));
                throw new KeyStoreException("The key entry do not exists.");
            }

            var result = await GetKeyValue(keyIdentifier, keClass, containerSelector, divInput);
            if (string.IsNullOrEmpty(result))
            {
                log.Warn("Key link returned an empty value.");
            }

            log.Info("Key link completed.");
            return result;
        }

        public event EventHandler<KeyEntry>? KeyEntryRetrieved;

        public event EventHandler<IChangeKeyEntry>? KeyEntryUpdated;
    }
}
