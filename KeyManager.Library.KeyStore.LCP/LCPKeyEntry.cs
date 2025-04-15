using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.LCP
{
    public class LCPKeyEntry : KeyEntry
    {
        public LCPKeyEntry() : this(KeyEntryClass.Symmetric)
        {

        }

        public LCPKeyEntry(KeyEntryClass kclass)
        {
            Properties = new LCPKeyEntryProperties();
            _kclass = kclass;
            Identifier.Id = Guid.NewGuid().ToString("N");
        }

        private readonly KeyEntryClass _kclass;

        [JsonIgnore]
        public LCPKeyEntryProperties? LCPProperties
        {
            get { return Properties as LCPKeyEntryProperties; }
        }

        public override KeyEntryClass KClass => _kclass;

        public override IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter)
        {
            var variants = new List<KeyEntryVariant>();

            if (classFilter == null || classFilter == KeyEntryClass.Symmetric)
            {
                var desvar = new KeyEntryVariant { Name = "DES" };
                desvar.KeyContainers.Add(new KeyVersion("Key", 0, new Key(new[] { "DES", KeyEntryClass.Symmetric.ToString() }, 16)));
                variants.Add(desvar);
                var tk3desvar = new KeyEntryVariant { Name = "TK3DES" };
                tk3desvar.KeyContainers.Add(new KeyVersion("Key", 0, new Key(new[] { "DES", KeyEntryClass.Symmetric.ToString() }, 24)));
                variants.Add(tk3desvar);
                var aes128var = new KeyEntryVariant { Name = "AES128" };
                aes128var.KeyContainers.Add(new KeyVersion("Key", 0,new Key(new[] { "AES", KeyEntryClass.Symmetric.ToString() }, 16)));
                variants.Add(aes128var);
                var aes256var = new KeyEntryVariant { Name = "AES256" };
                aes256var.KeyContainers.Add(new KeyVersion("Key", 0,new Key(new[] { "AES", KeyEntryClass.Symmetric.ToString() }, 32)));
                variants.Add(aes256var);
                var hmacvar = new KeyEntryVariant { Name = "HMAC" };
                hmacvar.KeyContainers.Add(new KeyVersion("Key", 0, new Key(new[] { "HMAC", KeyEntryClass.Symmetric.ToString() })));
                variants.Add(hmacvar);
            }

            if (classFilter == null || classFilter == KeyEntryClass.Asymmetric)
            {
                var rsapubvar = new KeyEntryVariant { Name = "RSA Public Key" };
                rsapubvar.KeyContainers.Add(new KeyVersion("Key", 0, new Key(new[] { "RSA", KeyEntryClass.Asymmetric.ToString(), KeyEntryClass.PublicKey.ToString() })));
                variants.Add(rsapubvar);
                var rsaprivar = new KeyEntryVariant { Name = "RSA Private Key" };
                rsaprivar.KeyContainers.Add(new KeyVersion("Key", 0, new Key(new[] { "RSA", KeyEntryClass.Asymmetric.ToString(), KeyEntryClass.PrivateKey.ToString() })));
                variants.Add(rsaprivar);
            }

            return variants;
        }

        public KeyEntryVariant? CreateVariantFromKeyType(string keyType)
        {
            keyType = keyType.ToLowerInvariant();
            string algo;
            uint keySize = 0;
            if (keyType == "aes128")
            {
                algo = "AES";
                keySize = 16;
            }
            else if (keyType == "aes256")
            {
                algo = "AES";
                keySize = 32;
            }
            else if (keyType == "2k3des")
            {
                algo = "DES";
                keySize = 16;
            }
            else if (keyType == "3k3des")
            {
                algo = "DES";
                keySize = 24;
            }
            else if (keyType == "rsa-public" || keyType == "rsa-private")
            {
                algo = "RSA";
            }
            else
            {
                algo = keyType;
            }
            return CreateVariantFromAlgo<KeyVersion>(algo, keySize);
        }

        public static string GetKeyTypeFromVariant(KeyEntryVariant variant)
        {
            var vname = variant.Name.ToLowerInvariant();
            string keyType;
            if (vname == "des")
            {
                keyType = "2k3des";
            }
            else if (vname == "tk3des")
            {
                keyType = "3k3des";
            }
            else
            {
                keyType = vname;
            }
            return keyType;
        }
    }
}
