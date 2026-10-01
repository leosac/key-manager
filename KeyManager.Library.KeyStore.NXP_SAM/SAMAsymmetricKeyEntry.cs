using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMAsymmetricKeyEntry : KeyEntry
    {
        public SAMAsymmetricKeyEntry()
        {
            Identifier.Id = "0";
            Properties = new SAMAsymmetricKeyEntryProperties();
        }

        [JsonIgnore]
        public SAMAsymmetricKeyEntryProperties? SAMProperties
        {
            get { return Properties as SAMAsymmetricKeyEntryProperties; }
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
                    new KeyMaterial("", KeyMaterial.PRIVATE_KEY) { ValueFormat = KeyValueStringFormat.Pem },
                    new KeyMaterial("", KeyMaterial.PUBLIC_KEY) { ValueFormat = KeyValueStringFormat.Pem }
                ])));
                variants.Add(rsavar);

                /*var eccvar = new KeyEntryVariant { Name = "ECC" };
                eccvar.KeyContainers.Add(new KeyContainer("Key", new Key(new[] { "ECC", KeyEntryClass.Asymmetric.ToString() }, 0,
                [
                    new KeyMaterial("", KeyMaterial.PUBLIC_KEY)
                ])));
                variants.Add(eccvar);*/
            }

            return variants;
        }
    }
}
