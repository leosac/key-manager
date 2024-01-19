using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMSymmetricKeyEntry : KeyEntry
    {
        public SAMSymmetricKeyEntry()
        {
            Identifier.Id = "0";
            Properties = new SAMSymmetricKeyEntryProperties();
        }

        [JsonIgnore]
        public SAMSymmetricKeyEntryProperties? SAMProperties
        {
            get { return Properties as SAMSymmetricKeyEntryProperties; }
        }

        public override KeyEntryClass KClass => KeyEntryClass.Symmetric;

        public override IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter)
        {
            var variants = new List<KeyEntryVariant>();

            if (classFilter == null || classFilter == KeyEntryClass.Symmetric)
            {
                var desvar = new KeyEntryVariant { Name = "DES" };
                desvar.KeyContainers.Add(new KeyVersion("Key Version A", 0, new Key(new[] { "DES", KeyEntryClass.Symmetric.ToString() }, 16)));
                desvar.KeyContainers.Add(new KeyVersion("Key Version B", 0, new Key(new[] { "DES", KeyEntryClass.Symmetric.ToString() }, 16)));
                desvar.KeyContainers.Add(new KeyVersion("Key Version C", 0, new Key(new[] { "DES", KeyEntryClass.Symmetric.ToString() }, 16)));
                variants.Add(desvar);
                var tk3desvar = new KeyEntryVariant { Name = "TK3DES" };
                tk3desvar.KeyContainers.Add(new KeyVersion("Key Version A", 0, new Key(new[] { "DES", KeyEntryClass.Symmetric.ToString() }, 24)));
                tk3desvar.KeyContainers.Add(new KeyVersion("Key Version B", 0, new Key(new[] { "DES", KeyEntryClass.Symmetric.ToString() }, 24)));
                variants.Add(tk3desvar);
                var aes128var = new KeyEntryVariant { Name = "AES128" };
                aes128var.KeyContainers.Add(new KeyVersion("Key Version A", 0, new Key(new[] { "AES", KeyEntryClass.Symmetric.ToString() }, 16)));
                aes128var.KeyContainers.Add(new KeyVersion("Key Version B", 0, new Key(new[] { "AES", KeyEntryClass.Symmetric.ToString() }, 16)));
                aes128var.KeyContainers.Add(new KeyVersion("Key Version C", 0, new Key(new[] { "AES", KeyEntryClass.Symmetric.ToString() }, 16)));
                variants.Add(aes128var);
                var aes192var = new KeyEntryVariant { Name = "AES192" };
                aes192var.KeyContainers.Add(new KeyVersion("Key Version A", 0, new Key(new[] { "AES", KeyEntryClass.Symmetric.ToString() }, 24)));
                aes192var.KeyContainers.Add(new KeyVersion("Key Version B", 0, new Key(new[] { "AES", KeyEntryClass.Symmetric.ToString() }, 24)));
                variants.Add(aes192var);
                var aes256var = new KeyEntryVariant { Name = "AES256" };
                aes256var.KeyContainers.Add(new KeyVersion("Key Version A", 0, new Key(new[] { "AES", KeyEntryClass.Symmetric.ToString() }, 32)));
                aes256var.KeyContainers.Add(new KeyVersion("Key Version B", 0, new Key(new[] { "AES", KeyEntryClass.Symmetric.ToString() }, 32)));
                variants.Add(aes256var);
                var mfvar = new KeyEntryVariant { Name = "MIFARE" };
                mfvar.KeyContainers.Add(new KeyVersion("Key Version A", 0, new Key(new[] { "MIFARE", KeyEntryClass.Symmetric.ToString() }, 12)));
                mfvar.KeyContainers.Add(new KeyVersion("Key Version B", 0, new Key(new[] { "MIFARE", KeyEntryClass.Symmetric.ToString() }, 12)));
                mfvar.KeyContainers.Add(new KeyVersion("Key Version C", 0, new Key(new[] { "MIFARE", KeyEntryClass.Symmetric.ToString() }, 12)));
                variants.Add(mfvar);
            }

            return variants;
        }
    }
}
