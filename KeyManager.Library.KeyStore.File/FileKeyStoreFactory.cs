using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.File.UI
{
    public class FileKeyStoreFactory : KeyStoreFactory
    {
        public override string Name => "File Key Store";

        public override KeyStore CreateKeyStore()
        {
            return new FileKeyStore();
        }

        public override Type GetPropertiesType()
        {
            return typeof(FileKeyStoreProperties);
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new FileKeyStoreProperties();
        }

        public override KeyStoreProperties? CreateKeyStoreProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<FileKeyStoreProperties>(serialized);
        }
    }
}
