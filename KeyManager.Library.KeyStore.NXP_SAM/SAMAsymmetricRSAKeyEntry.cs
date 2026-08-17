using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMAsymmetricRSAKeyEntry : KeyEntry
    {
        public SAMAsymmetricRSAKeyEntry()
        {
            Identifier.Id = "0";
            Properties = new SAMAsymmetricRSAKeyEntryProperties();
        }

        [JsonIgnore]
        public SAMAsymmetricRSAKeyEntryProperties? SAMProperties
        {
            get { return Properties as SAMAsymmetricRSAKeyEntryProperties; }
        }

        public override KeyEntryClass KClass => KeyEntryClass.Asymmetric;

        public override IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter)
        {
            var variants = new List<KeyEntryVariant>();

            if (classFilter == null || classFilter == KeyEntryClass.Asymmetric)
            {
                var rsavar = new KeyEntryVariant { Name = "RSA" };
                rsavar.KeyContainers.Add(new KeyContainer("Key", new Key(new[] { "RSA", KeyEntryClass.Asymmetric.ToString() }, 0,
                [
                    new KeyMaterial("", KeyMaterial.PRIVATE_KEY),
                    new KeyMaterial("", KeyMaterial.PUBLIC_KEY)
                ])));
                variants.Add(rsavar);
            }

            return variants;
        }
    }
}
