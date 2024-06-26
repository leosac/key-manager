namespace Leosac.KeyManager.Library.KeyStore.Memory
{
    public class MemoryKeyStore : KeyStore
    {
        private readonly IList<KeyEntry> _keyEntries;

        public MemoryKeyStore()
        {
            _keyEntries = new List<KeyEntry>();
        }

        public override string Name => "Memory";

        public override bool CanCreateKeyEntries => true;

        public override bool CanDeleteKeyEntries => true;

        public override bool CanReorderKeyEntries => true;

        public override IEnumerable<KeyEntryClass> SupportedClasses
        {
            get => new KeyEntryClass[] { KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric };
        }

        public override Task Open()
        {
            return Task.CompletedTask;
        }

        public override Task Close(bool secretCleanup = true)
        {
            return base.Close(secretCleanup);
        }

        public override Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            return CheckKeyEntryExists(identifier, keClass, out _);
        }

        protected Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass, out KeyEntry? keyEntry)
        {
            keyEntry = _keyEntries.SingleOrDefault(k => k.Identifier.Id == identifier.Id && k.KClass == keClass);
            if (keyEntry == null && !string.IsNullOrEmpty(identifier.Label))
            {
                keyEntry = _keyEntries.SingleOrDefault(k => k.Identifier.Label?.ToLowerInvariant() == identifier.Label?.ToLowerInvariant() && k.KClass == keClass);
            }
            return Task.FromResult(keyEntry != null);
        }

        public override async Task Create(IChangeKeyEntry change)
        {
            if (change is KeyEntry keyEntry)
            {
                if (await CheckKeyEntryExists(keyEntry))
                {
                    throw new KeyStoreException("A key entry with the same identifier already exists.");
                }

                _keyEntries.Add(keyEntry);
            }
            else
            {
                throw new KeyStoreException("Unsupported `change` parameter.");
            }
        }

        public override Task<KeyEntryId> Generate(KeyEntryId? identifier, KeyEntryClass keClass)
        {
            if (identifier == null)
            {
                identifier = new KeyEntryId();
            }

            var keyEntry = new MemoryKeyEntry
            {
                Identifier = identifier
            };
            if (keClass == KeyEntryClass.Symmetric)
            {
                keyEntry.Variant = keyEntry.GetAllVariants(keClass).FirstOrDefault(v => v.Name == "AES128");
            }
            else
            {
                keyEntry.Variant = keyEntry.GetAllVariants(keClass).FirstOrDefault(v => v.Name == "RSA");
            }
            return Generate(keyEntry);
        }

        public override async Task Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing)
        {
            if (!await CheckKeyEntryExists(identifier, keClass, out KeyEntry? keyEntry) && !ignoreIfMissing)
            {
                throw new KeyStoreException("The key entry do not exists.");
            }
            if (keyEntry != null)
            {
                _keyEntries.Remove(keyEntry);
            }
        }

        public override async Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            if (!await CheckKeyEntryExists(identifier, keClass, out KeyEntry? keyEntry))
            {
                throw new KeyStoreException("The key entry do not exists.");
            }
            return keyEntry;
        }

        public override Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass)
        {
            IList<KeyEntryId> list =  _keyEntries.Where(k => keClass == null || k.KClass == keClass).Select(k => k.Identifier).ToList();
            return Task.FromResult(list);
        }

        public override async Task MoveDown(KeyEntryId identifier, KeyEntryClass keClass)
        {
            var ke = await Get(identifier, keClass);
            if (ke != null)
            {
                var oldindex = _keyEntries.IndexOf(ke);
                if (oldindex > 0)
                {
                    _keyEntries.Remove(ke);
                    _keyEntries.Insert(oldindex - 1, ke);
                }
            }
        }

        public override async Task MoveUp(KeyEntryId identifier, KeyEntryClass keClass)
        {
            var ke = await Get(identifier, keClass);
            if (ke != null)
            {
                var oldindex = _keyEntries.IndexOf(ke);
                if (oldindex < _keyEntries.Count - 1)
                {
                    _keyEntries.Remove(ke);
                    _keyEntries.Insert(oldindex + 1, ke);
                }
            }
        }

        public override async Task Store(IList<IChangeKeyEntry> changes)
        {
            foreach (var change in changes)
            {
                await Update(change, true);
            }
        }

        public override async Task Update(IChangeKeyEntry change, bool ignoreIfMissing)
        {
            await Delete(change.Identifier, change.KClass, ignoreIfMissing);
            await Create(change);
            OnKeyEntryUpdated(change);
        }
    }
}
