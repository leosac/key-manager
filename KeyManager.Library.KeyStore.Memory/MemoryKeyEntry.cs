using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.Memory
{
    public class MemoryKeyEntry : KeyEntry
    {
        public MemoryKeyEntry()
        {
            Properties = new MemoryKeyEntryProperties();
        }

        public override KeyEntryClass KClass
        {
            get => GetKeyEntryClassFromFirstKeyVariant();
        }

        public override IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter = null)
        {
            var variants = new List<KeyEntryVariant>();

            if (classFilter == null || classFilter == KeyEntryClass.Symmetric)
            {
                var desvar = new KeyEntryVariant() { Name = "DES" };
                desvar.KeyContainers.Add(new KeyContainer("Key", new Key(new string[] { "DES", KeyEntryClass.Symmetric.ToString() }, 16)));
                variants.Add(desvar);
                var tk3desvar = new KeyEntryVariant() { Name = "TK3DES" };
                tk3desvar.KeyContainers.Add(new KeyContainer("Key", new Key(new string[] { "DES", KeyEntryClass.Symmetric.ToString() }, 24)));
                variants.Add(tk3desvar);
                var aes128var = new KeyEntryVariant() { Name = "AES128" };
                aes128var.KeyContainers.Add(new KeyContainer("Key", new Key(new string[] { "AES", KeyEntryClass.Symmetric.ToString() }, 16)));
                variants.Add(aes128var);
                var aes192var = new KeyEntryVariant() { Name = "AES192" };
                aes192var.KeyContainers.Add(new KeyContainer("Key", new Key(new string[] { "AES", KeyEntryClass.Symmetric.ToString() }, 24)));
                variants.Add(aes192var);
                var aes256var = new KeyEntryVariant() { Name = "AES256" };
                aes256var.KeyContainers.Add(new KeyContainer("Key", new Key(new string[] { "AES", KeyEntryClass.Symmetric.ToString() }, 32)));
                variants.Add(aes256var);
            }

            if (classFilter == null || classFilter == KeyEntryClass.Asymmetric)
            {
                var rsavar = new KeyEntryVariant() { Name = "RSA" };
                rsavar.KeyContainers.Add(new KeyContainer("Key", new Key(new string[] { "RSA", KeyEntryClass.Asymmetric.ToString() }, 0, new KeyMaterial[]
                {
                    new KeyMaterial("", KeyMaterial.PRIVATE_KEY),
                    new KeyMaterial("", KeyMaterial.PUBLIC_KEY)
                })));
                variants.Add(rsavar);
            }

            return variants;
        }
    }
}
