﻿using System;
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

        public override IEnumerable<KeyEntryClass> SupportedClasses
        {
            get => new KeyEntryClass[] { KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric };
        }

        public override void Open()
        {

        }

        public override void Close()
        {

        }

        public override bool CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            KeyEntry? keyEntry;
            return CheckKeyEntryExists(identifier, keClass, out keyEntry);
        }

        protected bool CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass, out KeyEntry? keyEntry)
        {
            keyEntry = _keyEntries.Where(k => k.Identifier == identifier && k.KClass == keClass).SingleOrDefault();
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

        public override void Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing = false)
        {
            KeyEntry? keyEntry;
            lock (_keyEntries)
            {
                if (!CheckKeyEntryExists(identifier, keClass, out keyEntry) && !ignoreIfMissing)
                    throw new KeyStoreException("The key entry do not exists.");
                if (keyEntry != null)
                {
                    _keyEntries.Remove(keyEntry);
                }
            }
        }

        public override KeyEntry? Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            KeyEntry? keyEntry;
            if (!CheckKeyEntryExists(identifier, keClass, out keyEntry))
                throw new KeyStoreException("The key entry do not exists.");
            return keyEntry;
        }

        public override IList<KeyEntryId> GetAll(KeyEntryClass? keClass = null)
        {
            return _keyEntries.Where(k => keClass == null || k.KClass == keClass).Select(k => k.Identifier).ToList();
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
                Delete(change.Identifier, change.KClass, ignoreIfMissing);
                Create(change);
                OnKeyEntryUpdated(change);
            }
        }

        public override string? ResolveKeyLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, byte keyVersion, string? divInput = null)
        {
            throw new NotSupportedException();
        }

        public override string? ResolveKeyEntryLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? divInput = null, KeyEntryId? wrappingKeyId = null, byte wrappingKeyVersion = 0)
        {
            throw new NotSupportedException();
        }
    }
}
