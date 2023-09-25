using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public class PKCSC11KeyStoreFactory : KeyStoreFactory
    {
        public override string Name => "HSM PKCS#11";

        public override KeyStore CreateKeyStore()
        {
            return new PKCS11KeyStore();
        }

        public override Type GetPropertiesType()
        {
            return typeof(PKCS11KeyStoreProperties);
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new PKCS11KeyStoreProperties();
        }

        public override KeyStoreProperties? CreateKeyStoreProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<PKCS11KeyStoreProperties>(serialized);
        }
    }
}
