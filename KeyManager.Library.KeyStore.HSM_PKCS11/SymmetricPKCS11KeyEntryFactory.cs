using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI
{
    public class SymmetricPKCS11KeyEntryFactory : KeyEntryFactory
    {
        public override string Name => "PKCS#11 Symmetric Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => new KeyEntryClass[] { KeyEntryClass.Symmetric };

        public override KeyEntry CreateKeyEntry()
        {
            return new SymmetricPKCS11KeyEntry();
        }

        public override Type GetPropertiesType()
        {
            return typeof(SymmetricPKCS11KeyEntryProperties);
        }

        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new SymmetricPKCS11KeyEntryProperties();
        }

        public override KeyEntryProperties? CreateKeyEntryProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<SymmetricPKCS11KeyEntryProperties>(serialized);
        }
    }
}
