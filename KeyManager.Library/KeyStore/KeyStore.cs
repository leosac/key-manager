using Leosac.KeyManager.Library.DivInput;
using Newtonsoft.Json;
using System.Text;
using static System.Formats.Asn1.AsnWriter;
using System.Threading;

namespace Leosac.KeyManager.Library.KeyStore
{
    /// <summary>
    /// The base class for a Key Store implementation.
    /// </summary>
    public abstract class KeyStore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public const string ATTRIBUTE_NAME = "name";
        public const string ATTRIBUTE_HEXNAME = "hexname";
        public const string ATTRIBUTE_PUBVAR = "pubvar";
        public const string ATTRIBUTE_HEXPUBVAR = "hexpubvar";

        protected readonly JsonSerializerSettings _jsonSettings;

        protected KeyStore()
        {
            _jsonSettings = KeyEntry.CreateJsonSerializerSettings();
            DefaultKeyEntries = new Dictionary<KeyEntryClass, KeyEntry?>();
            Attributes = new Dictionary<string, string>();
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
        /// True if key entries can have a custom label, false otherwise.
        /// </summary>
        public virtual bool CanDefineKeyEntryLabel => true;

        /// <summary>
        /// Get the supported key entry classes.
        /// </summary>
        public abstract IEnumerable<KeyEntryClass> SupportedClasses { get; }

        /// <summary>
        /// The key store properties.
        /// </summary>
        public KeyStoreProperties? Properties { get; set; }

        public IDictionary<KeyEntryClass, KeyEntry?> DefaultKeyEntries { get; set; }

        public IDictionary<string, string> Attributes { get; }

        public StoreOptions? Options { get; set; }

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
        public virtual Task Close(bool secretCleanup)
        {
            if (secretCleanup)
            {
                CleanupSecret();
            }
            return Task.CompletedTask;
        }

        public void CleanupSecret()
        {
            if (!string.IsNullOrEmpty(Properties?.Secret) && !Properties.StoreSecret)
            {
                Properties.Secret = string.Empty;
            }
        }

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
        /// Generate a new key entry.
        /// </summary>
        /// <param name="identifier">The new key entry identifier</param>
        /// <param name="keClass">The key entry class</param>
        /// <returns>The key entry identifier</returns>
        public virtual Task<KeyEntryId> Generate(KeyEntryId? identifier, KeyEntryClass keClass)
        {
            log.Error("The key store doesn't support key entry generation without specifing the target type.");
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generate a new key entry.
        /// </summary>
        /// <param name="keyEntry">The new key entry</param>
        /// <returns>The key entry identifier</returns>
        public virtual async Task<KeyEntryId> Generate(KeyEntry keyEntry)
        {
            if (keyEntry.Variant != null && keyEntry.KClass == KeyEntryClass.Symmetric)
            {
                foreach (var kv in keyEntry.Variant.KeyContainers)
                {
                    if (kv.Key.KeySize > 0)
                    {
                        foreach (var m in kv.Key.Materials)
                        {
                            m.SetValueAsBinary(KeyGeneration.Random(kv.Key.KeySize));
                        }
                    }
                }
            }
            else
            {
                log.Error(string.Format("The key store doesn't support key entry generation for class `{0}`.", keyEntry.KClass));
                throw new NotImplementedException();
            }

            await Create(keyEntry);
            return keyEntry.Identifier;
        }

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
        public virtual async Task Store(IList<IChangeKeyEntry> changes)
        {
            log.Info(string.Format("Storing `{0}` key entries...", changes.Count));

            foreach (var change in changes)
            {
                if (await CheckKeyEntryExists(change.Identifier, change.KClass))
                {
                    if (!(Options?.GenerateKeys).GetValueOrDefault(false))
                    {
                        await Update(change);
                    }
                    else
                    {
                        string msg = string.Format("Key Entry `{0}` already exists, skipping key generation update.", change.Identifier);
                        log.Info(msg);
                        OnUserMessageNotified(msg);
                    }
                }
                else
                {
                    if ((Options?.GenerateKeys).GetValueOrDefault(false))
                    {
                        if (change is KeyEntry ke)
                        {
                            await Generate(ke);
                        }
                        else
                        {
                            await Generate(change.Identifier, change.KClass);
                        }
                    }
                    else
                    {
                        await Create(change);
                    }
                }
            }

            log.Info("Key Entries storing completed.");
        }

        public virtual async Task Publish(KeyStore store, Func<string, KeyStore?> getFavoriteKeyStore, Func<KeyStore, string?, Task<bool>>? askForKeyStoreSecretIfRequired, Action<KeyStore, KeyEntryClass, int>? initCallback)
        {
            var classes = SupportedClasses;
            foreach (var keClass in classes)
            {
                await Publish(store, getFavoriteKeyStore, askForKeyStoreSecretIfRequired, keClass, initCallback);
            }
        }

        public virtual async Task Publish(KeyStore store, Func<string, KeyStore?> getFavoriteKeyStore, Func<KeyStore, string?, Task<bool>>? askForKeyStoreSecretIfRequired, KeyEntryClass keClass, Action<KeyStore, KeyEntryClass, int>? initCallback)
        {
            await Publish(store, getFavoriteKeyStore, askForKeyStoreSecretIfRequired, keClass, null, initCallback);
        }

        protected virtual async Task KeyEntriesAction(KeyStore store, Func<string, KeyStore?> getFavoriteKeyStore, Func<KeyStore, string?, Task<bool>>? askForKeyStoreSecretIfRequired, KeyEntryClass keClass, IEnumerable<KeyEntryId>? ids, Action<KeyStore, KeyEntryClass, int>? initCallback, Func<KeyStore, List<IChangeKeyEntry>, Task> action, bool connectToStore = true)
        {
            var changes = new List<IChangeKeyEntry>();
            if (ids == null)
            {
                ids = await GetAll(keClass);
            }
            initCallback?.Invoke(this, keClass, ids.Count());
            if (!string.IsNullOrEmpty(Options?.PublishVariable))
            {
                Attributes[ATTRIBUTE_PUBVAR] = Options.PublishVariable;
                Attributes[ATTRIBUTE_HEXPUBVAR] = Convert.ToHexString(Encoding.UTF8.GetBytes(Options.PublishVariable));
            }


            var usedStores = new List<KeyStore>();
            try
            {
                foreach (var id in ids)
                {
                    var entry = await Get(id, keClass);
                    if (entry != null)
                    {
                        var resolveKeyLinks = (Options?.ResolveKeyLinks).GetValueOrDefault(true);
                        var resolveVariables = (Options?.ResolveVariables).GetValueOrDefault(true);
                        entry.Identifier = entry.Identifier.Clone(resolveVariables ? Attributes : null);
                        if (entry.Link != null && entry.Link.KeyIdentifier.IsConfigured() && !string.IsNullOrEmpty(entry.Link.KeyStoreFavorite))
                        {
                            if (resolveKeyLinks)
                            {
                                var cryptogram = new KeyEntryCryptogram
                                {
                                    Identifier = entry.Identifier
                                    // TODO: we may want to have a different wrapping key per Cryptogram later on
                                };

                                var ks = getFavoriteKeyStore(entry.Link.KeyStoreFavorite);
                                if (ks != null)
                                {
                                    if (!usedStores.Contains(ks))
                                    {
                                        usedStores.Add(ks);
                                    }
                                    if (askForKeyStoreSecretIfRequired != null)
                                    {
                                        var c = await askForKeyStoreSecretIfRequired(ks, entry.Link.KeyStoreFavorite);
                                        if (!c)
                                        {
                                            throw new KeyStoreException("Missing secret for the linked key store.");
                                        }
                                    }
                                    await ks.Open();
                                    try
                                    {
                                        var divContext = new DivInput.DivInputContext
                                        {
                                            KeyStore = ks,
                                            KeyEntry = entry
                                        };
                                        cryptogram.Value = await ks.ResolveKeyEntryLink(entry.Link.KeyIdentifier.Clone(resolveVariables ? Attributes : null), keClass, ComputeDivInput(divContext, entry.Link.DivInput), entry.Link.WrappingKey);
                                    }
                                    finally
                                    {
                                        await ks.Close(false);
                                    }
                                }
                                changes.Add(cryptogram);
                            }
                            else
                            {
                                if (resolveVariables)
                                {
                                    entry.Link.KeyIdentifier = entry.Link.KeyIdentifier.Clone(Attributes);
                                }
                                changes.Add(entry);
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
                                        if (resolveKeyLinks)
                                        {
                                            var ks = getFavoriteKeyStore(kv.Key.Link.KeyStoreFavorite);
                                            if (ks != null)
                                            {
                                                if (!usedStores.Contains(ks))
                                                {
                                                    usedStores.Add(ks);
                                                }
                                                if (askForKeyStoreSecretIfRequired != null)
                                                {
                                                    var c = await askForKeyStoreSecretIfRequired(ks, kv.Key.Link.KeyStoreFavorite);
                                                    if (!c)
                                                    {
                                                        throw new KeyStoreException("Missing secret for the linked key store.");
                                                    }
                                                }
                                                await ks.Open();
                                                try
                                                {
                                                    var divContext = new DivInput.DivInputContext
                                                    {
                                                        KeyStore = ks,
                                                        KeyEntry = entry,
                                                        KeyContainer = kv
                                                    };
                                                    kv.Key.SetAggregatedValueAsString(await ks.ResolveKeyLink(kv.Key.Link.KeyIdentifier.Clone(resolveVariables ? Attributes : null), keClass, kv.Key.Link.ContainerSelector, ComputeDivInput(divContext, kv.Key.Link.DivInput)));
                                                }
                                                finally
                                                {

                                                    await ks.Close(false);
                                                }
                                            }
                                            // We remove link information from the being pushed key entry
                                            kv.Key.Link = new KeyLink();
                                        }
                                        else
                                        {
                                            if (resolveVariables)
                                            {
                                                kv.Key.Link.KeyIdentifier = kv.Key.Link.KeyIdentifier.Clone(Attributes);
                                            }
                                        }
                                    }
                                }
                            }
                            changes.Add(entry);
                        }
                    }
                }
            }
            finally
            {
                foreach (var ks in usedStores)
                {
                    ks.CleanupSecret();
                }
            }

            if (connectToStore)
            {
                await store.Open();
            }
            try
            {
                await action(store, changes);
            }
            finally
            {
                if (connectToStore)
                {
                    await store.Close(true);
                }
            }
        }

        public virtual Task Publish(KeyStore store, Func<string, KeyStore?> getFavoriteKeyStore, Func<KeyStore, string?, Task<bool>>? askForKeyStoreSecretIfRequired, KeyEntryClass keClass, IEnumerable<KeyEntryId>? ids, Action<KeyStore, KeyEntryClass, int>? initCallback)
        {
            return KeyEntriesAction(store, getFavoriteKeyStore, askForKeyStoreSecretIfRequired, keClass, ids, initCallback, new Func<KeyStore, List<IChangeKeyEntry>, Task>(async (s, changes) =>
            {
                if (!(Options?.DryRun).GetValueOrDefault(false))
                {
                    await s.Store(changes);
                }
                else
                {
                    log.Info("Dry Run, skipping the storage of key entries.");
                }
            }));
        }

        public virtual Task Import(KeyStore store, Func<string, KeyStore?> getFavoriteKeyStore, Func<KeyStore, string?, Task<bool>>? askForKeyStoreSecretIfRequired, KeyEntryClass keClass, IEnumerable<KeyEntryId>? ids, Action<KeyStore, KeyEntryClass, int>? initCallback)
        {
            store.Open();
            try
            {
                return store.KeyEntriesAction(this, getFavoriteKeyStore, askForKeyStoreSecretIfRequired, keClass, ids, initCallback, new Func<KeyStore, List<IChangeKeyEntry>, Task>(async (s, changes) =>
                {
                    if (!(Options?.DryRun).GetValueOrDefault(false))
                    {
                        await s.Store(changes);
                    }
                    else
                    {
                        log.Info("Dry Run, skipping the storage of key entries.");
                    }
                }), false);
            }
            finally
            {
                store.Close(true);
            }
        }

        public virtual Task Diff(KeyStore store, Func<string, KeyStore?> getFavoriteKeyStore, Func<KeyStore, string?, Task<bool>>? askForKeyStoreSecretIfRequired, KeyEntryClass keClass, IEnumerable<KeyEntryId>? ids, Action<KeyStore, KeyEntryClass, int>? initCallback)
        {
            return KeyEntriesAction(store, getFavoriteKeyStore, askForKeyStoreSecretIfRequired, keClass, ids, initCallback, new Func<KeyStore, List<IChangeKeyEntry>, Task>(async (s, changes) =>
            {
                uint missings = 0, diffs = 0;
                var details = string.Empty;

                foreach (KeyEntry c in changes)
                {
                    if (await store.CheckKeyEntryExists(c.Identifier, keClass))
                    {
                        var ke = await store.Get(c.Identifier, keClass);
                        if (ke != null)
                        {
                            if (JsonConvert.SerializeObject(ke.Properties) == JsonConvert.SerializeObject(c.Properties))
                            {
                                if (c.Variant?.Name == ke.Variant?.Name)
                                {
                                    if (c.Variant != null)
                                    {
                                        for(int i = 0; i < c.Variant.KeyContainers.Count; i++)
                                        {
                                            if (c.Variant.KeyContainers[i].Key.GetAggregatedValueAsString() != ke.Variant!.KeyContainers[i].Key.GetAggregatedValueAsString())
                                            {
                                                diffs++;
                                                details += string.Format("Key Container `{0}` (#{1}) of {2} doesn't match.", c.Variant.KeyContainers[i].Name, i, c.Identifier) + Environment.NewLine;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    diffs++;
                                    details += string.Format("Variant of {0} doesn't match.", c.Identifier) + Environment.NewLine;
                                }
                            }
                            else
                            {
                                diffs++;
                                details += string.Format("Properties of {0} doesn't match.", c.Identifier) + Environment.NewLine;
                            }
                        }
                        else
                        {
                            diffs++;
                            details += string.Format("Cannot load details of {0}.", c.Identifier) + Environment.NewLine;
                        }
                    }
                    else
                    {
                        missings++;
                        details += string.Format("{0} is missing.", c.Identifier) + Environment.NewLine;
                    }
                }

                if (missings > 0 || diffs > 0)
                {
                    var differror = "Key Entries on both Key Store doesn't match." + Environment.NewLine
                                       + string.Format("Missing: {0} - Differences: {1}", missings, diffs) + Environment.NewLine
                                       +  details;
                    log.Info(differror);
                    throw new KeyStoreException(differror);
                }
            }));
        }

        private static string? ComputeDivInput(DivInputContext divContext, IList<DivInputFragment> divInput)
        {
            divContext.CurrentDivInput = null;
            if (divInput != null && divInput.Count > 0)
            {
                divContext.CurrentDivInput = string.Empty;
                foreach (var input in divInput)
                {
                    divContext.CurrentDivInput += input.GetFragment(divContext);
                }
            }
            return divContext.CurrentDivInput;
        }

        protected void OnKeyEntryRetrieved(KeyEntry keyEntry)
        {
            KeyEntryRetrieved?.Invoke(this, keyEntry);
        }

        protected void OnKeyEntryUpdated(IChangeKeyEntry keyEntry)
        {
            KeyEntryUpdated?.Invoke(this, keyEntry);
        }

        protected void OnUserMessageNotified(string message)
        {
            UserMessageNotified?.Invoke(this, message);
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
            return key?.GetAggregatedValueAsString();
        }

        /// <summary>
        /// Resolve a key entry link.
        /// </summary>
        /// <param name="keyIdentifier">The key entry identifier</param>
        /// <param name="keClass">The key entry class</param>
        /// <param name="divInput">The key div input (optional)</param>
        /// <param name="wrappingKey">The wrapping key for cryptogram computation (optional)</param>
        /// <returns>The change key entry cryptogram</returns>
        public virtual async Task<string?> ResolveKeyEntryLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? divInput, WrappingKey? wrappingKey)
        {
            string? result = null;
            log.Info(string.Format("Resolving key entry link with Key Entry Identifier `{0}`, Div Input `{1}`...", keyIdentifier, divInput));

            var keyEntry = await Get(keyIdentifier, keClass);
            if (keyEntry != null)
            {
                log.Info("Key entry link resolved.");
                if (wrappingKey != null && wrappingKey.KeyId.IsConfigured())
                {
                    var wKey = await GetKey(wrappingKey.KeyId, KeyEntryClass.Symmetric, wrappingKey.ContainerSelector);
                    if (wKey != null)
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

        public virtual KeyEntry? GetDefaultKeyEntry(KeyEntryClass keClass)
        {
            return GetDefaultKeyEntry(keClass, true);
        }

        public KeyEntry? GetDefaultKeyEntry(KeyEntryClass keClass, bool clone)
        {
            if (DefaultKeyEntries.ContainsKey(keClass))
            {
                return clone ? DefaultKeyEntries[keClass]?.DeepCopy() : DefaultKeyEntries[keClass];
            }

            return null;
        }

        public event EventHandler<KeyEntry>? KeyEntryRetrieved;

        public event EventHandler<IChangeKeyEntry>? KeyEntryUpdated;

        public event EventHandler<string>? UserMessageNotified;
    }
}
