using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.KeePass
{
    public class KeePassKeyStoreFactory : KeyStoreFactory
    {
        public override string Name => "KeePass Key Store";

        public override KeyStore CreateKeyStore()
        {
            return new KeePassKeyStore();
        }

        public override Type? GetPropertiesType()
        {
            return typeof(KeePassKeyStoreProperties);
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new KeePassKeyStoreProperties();
        }

        public override KeyStoreProperties? CreateKeyStoreProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<KeePassKeyStoreProperties>(serialized);
        }

    }
}
