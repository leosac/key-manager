using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SEKeyStoreFactory : KeyStoreFactory
    {
        public override string Name => "SAM-SE Key Store";

        public override KeyStore CreateKeyStore()
        {
            return new SAM_SEKeyStore();
        }

        public override Type GetPropertiesType()
        {
            return typeof(SAM_SEKeyStoreProperties);
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new SAM_SEKeyStoreProperties();
        }

        public override KeyStoreProperties? CreateKeyStoreProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<SAM_SEKeyStoreProperties>(serialized);
        }
    }
}
