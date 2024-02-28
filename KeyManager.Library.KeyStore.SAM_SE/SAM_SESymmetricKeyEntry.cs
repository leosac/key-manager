using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntry : KeyEntry
    {
        public SAM_SESymmetricKeyEntry() : base()
        {
            Identifier.Id = "0000";
            Properties = new SAM_SESymmetricKeyEntryProperties();
            AddVariants();
        }

        public SAM_SESymmetricKeyEntry(KeyEntryId keyEntryId) : base()
        {
            Identifier = keyEntryId;
            Properties = new SAM_SESymmetricKeyEntryProperties();
            AddVariants();
        }

        public SAM_SESymmetricKeyEntry(KeyEntryId keyEntryId, SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType type) : base()
        {
            Identifier = keyEntryId;
            Properties = new SAM_SESymmetricKeyEntryProperties();
            SAM_SEProperties!.KeyEntryType = type;
            AddVariants();
        }

        public SAM_SESymmetricKeyEntry(string keyEntryId, SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType type) : base()
        {
            Identifier = new(keyEntryId);
            Properties = new SAM_SESymmetricKeyEntryProperties();
            SAM_SEProperties!.KeyEntryType = type;
            AddVariants();
        }

        [JsonIgnore]
        public SAM_SESymmetricKeyEntryProperties? SAM_SEProperties
        {
            get { return Properties as SAM_SESymmetricKeyEntryProperties; }
        }

        public override KeyEntryClass KClass => KeyEntryClass.Symmetric;

        public override IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter = null)
        {
            var variants = new List<KeyEntryVariant>();

            if (classFilter == null || classFilter == KeyEntryClass.Symmetric)
            {
                var DESFirevar = new KeyEntryVariant() { Name = "AES128" };
                DESFirevar.KeyContainers.Add(new KeyVersion("Key value", 1, new Key(new string[] { "AES", KeyEntryClass.Symmetric.ToString() }, 16)));
                variants.Add(DESFirevar);
            }

            return variants;
        }

        private void AddVariants()
        {
            var DESFirevar = new KeyEntryVariant() { Name = "AES128" };
            //We do not add a Variant for SCP3 since its a specific object
            //In pratic there is a key, calculated from a password
            //But to avoid confusion, no variant for SCP3
            if (Identifier.Id != "SCP3")
                DESFirevar.KeyContainers.Add(new KeyVersion("Key value", 1, new Key(new string[] { "AES", KeyEntryClass.Symmetric.ToString() }, 16)));
            Variant = DESFirevar;
        }
    }
}
