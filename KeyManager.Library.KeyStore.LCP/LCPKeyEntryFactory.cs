using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.LCP
{
    public class LCPKeyEntryFactory : KeyEntryFactory
    {
        public override string Name => "LCP Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => new KeyEntryClass[] { KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric };

        public override KeyEntry CreateKeyEntry()
        {
            return new LCPKeyEntry();
        }

        public override Type GetPropertiesType()
        {
            return typeof(LCPKeyEntryProperties);
        }

        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new LCPKeyEntryProperties();
        }

        public override KeyEntryProperties? CreateKeyEntryProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<LCPKeyEntryProperties>(serialized);
        }
    }
}
