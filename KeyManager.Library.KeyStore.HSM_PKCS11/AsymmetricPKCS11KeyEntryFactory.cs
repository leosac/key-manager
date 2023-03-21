using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI
{
    public class AsymmetricPKCS11KeyEntryFactory : KeyEntryFactory
    {
        public override string Name => "PKCS#11 Asymmetric Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => new KeyEntryClass[] { KeyEntryClass.Asymmetric };

        public override KeyEntry CreateKeyEntry()
        {
            return new AsymmetricPKCS11KeyEntry();
        }

        public override Type GetPropertiesType()
        {
            return typeof(AsymmetricPKCS11KeyEntryProperties);
        }

        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new AsymmetricPKCS11KeyEntryProperties();
        }

        public override KeyEntryProperties? CreateKeyEntryProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<AsymmetricPKCS11KeyEntryProperties>(serialized);
        }
    }
}
