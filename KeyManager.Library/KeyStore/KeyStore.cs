using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public abstract class KeyStore
    {
        public abstract string Name { get; }

        public abstract bool CanCreateKeyEntries { get; }

        public abstract bool CanDeleteKeyEntries { get; }

        public KeyStoreProperties? Properties { get; set; }

        public bool CreateIfMissing { get; set; } = false;

        public bool CheckKeyEntryExists(KeyEntry keyEntry)
        {
            return CheckKeyEntryExists(keyEntry.Identifier);
        }

        public abstract void Open();

        public abstract void Close();

        public abstract bool CheckKeyEntryExists(string identifier);

        public abstract IList<string> GetAll();

        public abstract void Create(KeyEntry keyEntry);

        public abstract KeyEntry? Get(string identifier);

        public abstract void Update(KeyEntry keyEntry, bool ignoreIfMissing = false);

        public abstract void Delete(string identifier, bool ignoreIfMissing = false);

        public virtual void Store(KeyEntry keyEntry)
        {
            var list = new List<KeyEntry>();
            list.Add(keyEntry);
            Store(list);
        }

        public abstract void Store(IList<KeyEntry> keyEntries);

        protected void OnKeyEntryUpdated(KeyEntry keyEntry)
        {
            if (KeyEntryUpdated != null)
            {
                KeyEntryUpdated(this, keyEntry);
            }
        }

        public event EventHandler<KeyEntry>? KeyEntryUpdated;
    }
}
