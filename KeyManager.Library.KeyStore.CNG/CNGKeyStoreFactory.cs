using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.CNG
{
    public class CNGKeyStoreFactory : KeyStoreFactory
    {
        public override string Name => "Cryptography API: Next Generation (CNG)";

        public override KeyStore CreateKeyStore()
        {
            return new CNGKeyStore();
        }

        public override Type GetPropertiesType()
        {
            return typeof(CNGKeyStoreProperties);
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new CNGKeyStoreProperties();
        }

        public override KeyStoreProperties? CreateKeyStoreProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<CNGKeyStoreProperties>(serialized);
        }
    }
}
