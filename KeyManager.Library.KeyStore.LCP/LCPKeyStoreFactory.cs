using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.LCP
{
    public class LCPKeyStoreFactory : KeyStoreFactory
    {
        public override string Name => "Leosac Credential Provisioning Server";

        public override KeyStore CreateKeyStore()
        {
            return new LCPKeyStore();
        }

        public override Type GetPropertiesType()
        {
            return typeof(LCPKeyStoreProperties);
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new LCPKeyStoreProperties();
        }

        public override KeyStoreProperties? CreateKeyStoreProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<LCPKeyStoreProperties>(serialized);
        }
    }
}
