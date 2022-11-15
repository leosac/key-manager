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
        public const string LeosacKeyFileExtension = ".leok";

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public FileKeyStore()
        {
            Properties = new FileKeyStoreProperties();
            _jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                Formatting = Formatting.Indented
            };
        }

        JsonSerializerSettings _jsonSettings;

        public FileKeyStoreProperties GetFileProperties()
        {
            var p = Properties as FileKeyStoreProperties;
            if (p == null)
                throw new KeyStoreException("Missing File key store properties.");
            return p;
        }

        public override string Name => "File";

        public override bool CanCreateKeyEntries => true;

        public override bool CanDeleteKeyEntries => true;

        public override void Open()
        {
            log.Info(String.Format("Opening the key store `{0}`...", GetFileProperties().Fullpath));
            if (!System.IO.Directory.Exists(GetFileProperties().Fullpath))
            {
                if (CreateIfMissing)
                    System.IO.Directory.CreateDirectory(GetFileProperties().Fullpath);
                else
                {
                    log.Error(String.Format("Cannot open the key sore `{0}`.", GetFileProperties().Fullpath));
                    throw new KeyStoreException("Cannot open the key sore.");
                }
            }
            log.Info("Key store opened.");
        }

        public override void Close()
        {
            log.Info(String.Format("Closing the key store `{0}`...", GetFileProperties().Fullpath));
            log.Info("Key Store closed.");
        }

        protected string GetKeyEntryFile(string identifier)
        {
            return System.IO.Path.Combine(GetFileProperties().Fullpath, identifier + LeosacKeyFileExtension);
        }

        public override bool CheckKeyEntryExists(string identifier)
        {
            return System.IO.File.Exists(GetKeyEntryFile(identifier));
        }

        public override void Create(KeyEntry keyEntry)
        {
            log.Info(String.Format("Creating key entry `{0}`...", keyEntry.Identifier));
            if (CheckKeyEntryExists(keyEntry.Identifier))
            {
                log.Error(String.Format("A key entry with the same identifier `{0}` already exists.", keyEntry.Identifier));
                throw new KeyStoreException("A key entry with the same identifier already exists.");
            }

            string serialized = JsonConvert.SerializeObject(keyEntry, typeof(KeyEntry), _jsonSettings);
            System.IO.File.WriteAllText(GetKeyEntryFile(keyEntry.Identifier), serialized);
            log.Info(String.Format("Kkey entry `{0}` created.", keyEntry.Identifier));
        }

        public override void Delete(string identifier, bool ignoreIfMissing = false)
        {
            log.Info(String.Format("Deleting key entry `{0}`...", identifier));
            var exists = CheckKeyEntryExists(identifier);
            if (!exists && !ignoreIfMissing)
            {
                log.Error(String.Format("The key entry `{0}` do not exists.", identifier));
                throw new KeyStoreException("The key entry do not exists.");
            }

            if (exists)
            {
                System.IO.File.Delete(GetKeyEntryFile(identifier));
                log.Info(String.Format("Key entry `{0}` deleted.", identifier));
            }
        }

        public override KeyEntry? Get(string identifier)
        {
            log.Info(String.Format("Getting key entry `{0}`...", identifier));
            if (!CheckKeyEntryExists(identifier))
            {
                log.Error(String.Format("The key entry `{0}` do not exists.", identifier));
                throw new KeyStoreException("The key entry do not exists.");
            }

            string serialized = System.IO.File.ReadAllText(GetKeyEntryFile(identifier));
            var keyEntry = JsonConvert.DeserializeObject<KeyEntry>(serialized, _jsonSettings);
            log.Info(String.Format("Key entry `{0}` retrieved.", identifier));
            return keyEntry;
        }

        public override IList<string> GetAll()
        {
            log.Info("Getting all key entries...");
            var keyEntries = new List<string>();
            var files = System.IO.Directory.GetFiles(GetFileProperties().Fullpath, "*" + LeosacKeyFileExtension);
            foreach (var file in files)
            {
                string identifier = System.IO.Path.GetFileNameWithoutExtension(file);
                keyEntries.Add(identifier);
            }
            log.Info(String.Format("{0} key entries returned.", keyEntries.Count));
            return keyEntries;
        }

        public override void Store(IList<KeyEntry> keyEntries)
        {
            log.Info(String.Format("Storing `{0}` key entries...", keyEntries.Count));
            foreach (var keyEntry in keyEntries)
            {
                Update(keyEntry, true);
            }
            log.Info("Key Entries storing completed.");
        }

        public override void Update(KeyEntry keyEntry, bool ignoreIfMissing = false)
        {
            log.Info(String.Format("Updating key entry `{0}`...", keyEntry.Identifier));
            Delete(keyEntry.Identifier, ignoreIfMissing);
            Create(keyEntry);
            OnKeyEntryUpdated(keyEntry);
            log.Info(String.Format("Key entry `{0}` updated.", keyEntry.Identifier));
        }
    }
}
