using Leosac.KeyManager.Library.KeyStore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library
{
    public abstract class KMPermanentConfig<T> where T : KMPermanentConfig<T>, new()
    {
        static JsonSerializerSettings _jsonSettings;

        static KMPermanentConfig()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                Formatting = Formatting.Indented
            };
        }

        public abstract string GetDefaultFileName();

        public void SaveToFile()
        {
            SaveToFile(GetConfigFilePath(true));
        }

        public void SaveToFile(string filePath)
        {
            var serialized = JsonConvert.SerializeObject(this, _jsonSettings);
            System.IO.File.WriteAllText(filePath, serialized, Encoding.UTF8);
        }

        public static T? LoadFromFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                var serialized = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
                return JsonConvert.DeserializeObject<T>(serialized, _jsonSettings);
            }
            else
            {
                return new T();
            }
        }

        public string GetConfigFilePath(bool createFolders = false)
        {
            return GetConfigFilePath(GetDefaultFileName(), createFolders);
        }

        public static string GetConfigFilePath(string fileName, bool createFolders = false)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appData, "Leosac");
            if (!Directory.Exists(path) && createFolders)
                Directory.CreateDirectory(path);

            path = Path.Combine(path, "Key Manager");
            if (!Directory.Exists(path) && createFolders)
                Directory.CreateDirectory(path);

            return Path.Combine(path, fileName);
        }
    }
}
