using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.Memory
{
    public class MemorySymmetricKeyEntry : SymmetricKeyEntry
    {
        public MemorySymmetricKeyEntry()
        {
            Properties = new MemoryKeyEntryProperties();
        }

        public override IList<KeyEntryVariant> GetAllVariants()
        {
            var variants = new List<KeyEntryVariant>();

            var desvar = new KeyEntryVariant() { Name = "DES" };
            desvar.KeyVersions.Add(new KeyVersion("Key", 0, new Key(KeyTag.DES, 16)));
            variants.Add(desvar);
            var tk3desvar = new KeyEntryVariant() { Name = "TK3DES" };
            tk3desvar.KeyVersions.Add(new KeyVersion("Key", 0, new Key(KeyTag.DES, 24)));
            variants.Add(tk3desvar);
            var aes128var = new KeyEntryVariant() { Name = "AES128" };
            aes128var.KeyVersions.Add(new KeyVersion("Key", 0, new Key(KeyTag.AES, 16)));
            variants.Add(aes128var);
            var aes192var = new KeyEntryVariant() { Name = "AES192" };
            aes192var.KeyVersions.Add(new KeyVersion("Key", 0, new Key(KeyTag.AES, 24)));
            variants.Add(aes192var);
            var aes256var = new KeyEntryVariant() { Name = "AES256" };
            aes256var.KeyVersions.Add(new KeyVersion("Key", 0, new Key(KeyTag.AES, 32)));
            variants.Add(aes256var);

            return variants;
        }
    }
}
