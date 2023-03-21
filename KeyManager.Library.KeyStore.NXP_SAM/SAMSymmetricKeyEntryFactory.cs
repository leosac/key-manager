using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMSymmetricKeyEntryFactory : KeyEntryFactory
    {
        public override string Name => "NXP SAM Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => new KeyEntryClass[] { KeyEntryClass.Symmetric };

        public override KeyEntry CreateKeyEntry()
        {
            return new SAMSymmetricKeyEntry();
        }

        public override Type GetPropertiesType()
        {
            return typeof(SAMSymmetricKeyEntryProperties);
        }

        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new SAMSymmetricKeyEntryProperties();
        }

        public override KeyEntryProperties? CreateKeyEntryProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<SAMSymmetricKeyEntryProperties>(serialized);
        }
    }
}
