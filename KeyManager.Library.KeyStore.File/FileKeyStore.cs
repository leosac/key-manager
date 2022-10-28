using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.File
{
    public class FileKeyStore : KeyStore
    {
        JsonSerializerSettings _jsonSettings;

        public FileKeyStore()
        {
            Properties = new FileKeyStoreProperties();
            _jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        }

        public FileKeyStoreProperties GetFileProperties()
        {
            var p = Properties as FileKeyStoreProperties;
            if (p == null)
                throw new KeyStoreException("Missing File key store properties.");
            return p;
        }

        public override string Name => "File";

        protected string GetKeyEntryFile(string identifier)
        {
            return System.IO.Path.Combine(GetFileProperties().Directory, identifier);
        }

        public override bool CheckKeyEntryExists(string identifier)
        {
            return System.IO.File.Exists(GetKeyEntryFile(identifier));
        }

        public override void Create(KeyEntry keyEntry)
        {
            if (!CheckKeyEntryExists(keyEntry.Identifier))
                throw new KeyStoreException("A key entry with the same identifier already exists.");

            string serialized = JsonConvert.SerializeObject(keyEntry, _jsonSettings);
            System.IO.File.WriteAllText(GetKeyEntryFile(keyEntry.Identifier), serialized);
        }

        public override void Delete(string identifier, bool ignoreIfMissing = false)
        {
            var exists = CheckKeyEntryExists(identifier);
            if (!exists && !ignoreIfMissing)
                throw new KeyStoreException("The key entry do not exists.");

            if (exists)
            {
                System.IO.File.Delete(GetKeyEntryFile(identifier));
            }
        }

        public override KeyEntry? Get(string identifier)
        {
            if (!CheckKeyEntryExists(identifier))
                throw new KeyStoreException("The key entry do not exists.");

            string serialized = System.IO.File.ReadAllText(GetKeyEntryFile(identifier));
            return JsonConvert.DeserializeObject<KeyEntry>(serialized);
        }

        public override IList<string> GetAll()
        {
            var keyEntries = new List<string>();
            var files = System.IO.Directory.GetFiles(GetFileProperties().Directory);
            foreach (var file in files)
            {
                string identifier = System.IO.Path.GetFileName(file);
                keyEntries.Add(identifier);
            }
            return keyEntries;
        }

        public override void Store(IList<KeyEntry> keyEntries)
        {
            if (!System.IO.Directory.Exists(GetFileProperties().Directory))
                System.IO.Directory.CreateDirectory(GetFileProperties().Directory);

            foreach (var keyEntry in keyEntries)
            {
                Update(keyEntry, true);
            }
        }

        public override void Update(KeyEntry keyEntry, bool ignoreIfMissing = false)
        {
            Delete(keyEntry.Identifier, ignoreIfMissing);
            Create(keyEntry);
        }
    }
}
