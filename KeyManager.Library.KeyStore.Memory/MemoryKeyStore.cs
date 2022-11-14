using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.Memory
{
    public class MemoryKeyStore : KeyStore
    {
        IList<KeyEntry> _keyEntries;

        public MemoryKeyStore()
        {
            _keyEntries = new List<KeyEntry>();
        }

        public override string Name => "Memory";

        public override bool CanCreateKeyEntries => true;

        public override bool CanDeleteKeyEntries => true;

        public override void Open()
        {

        }

        public override void Close()
        {

        }

        public override bool CheckKeyEntryExists(string identifier)
        {
            KeyEntry? keyEntry;
            return CheckKeyEntryExists(identifier, out keyEntry);
        }

        protected bool CheckKeyEntryExists(string identifier, out KeyEntry? keyEntry)
        {
            keyEntry = _keyEntries.Where(k => k.Identifier == identifier).SingleOrDefault();
            return (keyEntry != null);
        }

        public override void Create(KeyEntry keyEntry)
        {
            lock (_keyEntries)
            {
                if (CheckKeyEntryExists(keyEntry))
                    throw new KeyStoreException("A key entry with the same identifier already exists.");

                _keyEntries.Add(keyEntry);
            }
        }

        public override void Delete(string identifier, bool ignoreIfMissing = false)
        {
            KeyEntry? keyEntry;
            lock (_keyEntries)
            {
                if (!CheckKeyEntryExists(identifier, out keyEntry) && !ignoreIfMissing)
                    throw new KeyStoreException("The key entry do not exists.");
                if (keyEntry != null)
                {
                    _keyEntries.Remove(keyEntry);
                }
            }
        }

        public override KeyEntry? Get(string identifier)
        {
            KeyEntry? keyEntry;
            if (!CheckKeyEntryExists(identifier, out keyEntry))
                throw new KeyStoreException("The key entry do not exists.");
            return keyEntry;
        }

        public override IList<string> GetAll()
        {
            return _keyEntries.Select(k => k.Identifier).ToList();
        }

        public override void Store(IList<KeyEntry> keyEntries)
        {
            lock (_keyEntries)
            {
                _keyEntries.UnionBy(keyEntries, k => k.Identifier);
            }
        }

        public override void Update(KeyEntry keyEntry, bool ignoreIfMissing = false)
        {
            lock (_keyEntries)
            {
                Delete(keyEntry.Identifier, ignoreIfMissing);
                Create(keyEntry);
                OnKeyEntryUpdated(keyEntry);
            }
        }
    }
}
