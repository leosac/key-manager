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

        public override Task Close()
        {
            return Task.CompletedTask;
        }

        public override Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            return CheckKeyEntryExists(identifier, keClass, out _);
        }

        protected Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass, out KeyEntry? keyEntry)
        {
            keyEntry = _keyEntries.Where(k => k.Identifier == identifier && k.KClass == keClass).SingleOrDefault();
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

        public override Task<string?> ResolveKeyLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? containerSelector, string? divInput)
        {
            throw new NotSupportedException();
        }

        public override Task<string?> ResolveKeyEntryLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? divInput, KeyEntryId? wrappingKeyId, string? wrappingContainerSelector)
        {
            throw new NotSupportedException();
        }
    }
}
