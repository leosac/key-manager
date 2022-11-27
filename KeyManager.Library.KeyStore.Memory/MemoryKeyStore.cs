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

        public override bool CheckKeyEntryExists(KeyEntryId identifier)
        {
            KeyEntry? keyEntry;
            return CheckKeyEntryExists(identifier, out keyEntry);
        }

        protected bool CheckKeyEntryExists(KeyEntryId identifier, out KeyEntry? keyEntry)
        {
            keyEntry = _keyEntries.Where(k => k.Identifier == identifier).SingleOrDefault();
            return (keyEntry != null);
        }

        public override void Create(IChangeKeyEntry change)
        {
            lock (_keyEntries)
            {
                if (change is KeyEntry keyEntry)
                {
                    if (CheckKeyEntryExists(keyEntry))
                        throw new KeyStoreException("A key entry with the same identifier already exists.");

                    _keyEntries.Add(keyEntry);
                }
                else
                    throw new KeyStoreException("Unsupported `change` parameter.");
            }
        }

        public override void Delete(KeyEntryId identifier, bool ignoreIfMissing = false)
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

        public override KeyEntry? Get(KeyEntryId identifier)
        {
            KeyEntry? keyEntry;
            if (!CheckKeyEntryExists(identifier, out keyEntry))
                throw new KeyStoreException("The key entry do not exists.");
            return keyEntry;
        }

        public override IList<KeyEntryId> GetAllSymmetric()
        {
            return _keyEntries.Select(k => k.Identifier).ToList();
        }

        public override void Store(IList<IChangeKeyEntry> changes)
        {
            lock (_keyEntries)
            {
                foreach (var change in changes)
                {
                    Update(change, true);
                }
            }
        }

        public override void Update(IChangeKeyEntry change, bool ignoreIfMissing = false)
        {
            lock (_keyEntries)
            {
                Delete(change.Identifier, ignoreIfMissing);
                Create(change);
                OnKeyEntryUpdated(change);
            }
        }

        public override string? ResolveKeyLink(KeyEntryId keyIdentifier, byte keyVersion, string? divInput = null)
        {
            throw new NotSupportedException();
        }

        public override string? ResolveKeyEntryLink(KeyEntryId keyIdentifier, string? divInput = null, KeyEntryId? wrappingKeyId = null, byte wrappingKeyVersion = 0)
        {
            throw new NotSupportedException();
        }
    }
}
