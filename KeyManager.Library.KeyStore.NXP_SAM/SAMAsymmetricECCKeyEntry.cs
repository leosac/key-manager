using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMAsymmetricECCKeyEntry : KeyEntry
    {
        public SAMAsymmetricECCKeyEntry()
        {
            Identifier.Id = "0";
            Properties = new SAMAsymmetricECCKeyEntryProperties();
        }

        [JsonIgnore]
        public SAMAsymmetricECCKeyEntryProperties? SAMProperties
        {
            get { return Properties as SAMAsymmetricECCKeyEntryProperties; }
        }

        public override KeyEntryClass KClass => KeyEntryClass.Asymmetric;

        public override IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter)
        {
            var variants = new List<KeyEntryVariant>();

            if (classFilter == null || classFilter == KeyEntryClass.Asymmetric)
            {
                var eccvar = new KeyEntryVariant { Name = "ECC" };
                eccvar.KeyContainers.Add(new KeyContainer("Key", new Key(new[] { "ECC", KeyEntryClass.Asymmetric.ToString() }, 0,
                [
                    new KeyMaterial("", KeyMaterial.PUBLIC_KEY)
                ])));
                variants.Add(eccvar);
            }

            return variants;
        }
    }
}
