using Leosac.KeyManager.Library.KeyStore.CNG;
using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.CNG
{
    public class CNGKeyEntryFactory : KeyEntryFactory
    {
        public override string Name => "CNG Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => [KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric];

        public override KeyEntry CreateKeyEntry()
        {
            return new CNGKeyEntry();
        }

        public override Type GetPropertiesType()
        {
            return typeof(CNGKeyEntryProperties);
        }

        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new CNGKeyEntryProperties();
        }

        public override KeyEntryProperties? CreateKeyEntryProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<CNGKeyEntryProperties>(serialized);
        }
    }
}
