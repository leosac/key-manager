using LibLogicalAccess.Card;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMSymmetricKeyEntry : KeyEntry
    {
        public SAMSymmetricKeyEntry() : base()
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

        public override IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter = null)
        {
            var variants = new List<KeyEntryVariant>();

            if (classFilter == null || classFilter == KeyEntryClass.Symmetric)
            {
                var desvar = new KeyEntryVariant() { Name = "DES" };
                desvar.KeyContainers.Add(new KeyVersion("Key Version A", 0, new Key(new string[] { "DES", KeyEntryClass.Symmetric.ToString() }, 16)));
                desvar.KeyContainers.Add(new KeyVersion("Key Version B", 0, new Key(new string[] { "DES", KeyEntryClass.Symmetric.ToString() }, 16)));
                desvar.KeyContainers.Add(new KeyVersion("Key Version C", 0, new Key(new string[] { "DES", KeyEntryClass.Symmetric.ToString() }, 16)));
                variants.Add(desvar);
                var tk3desvar = new KeyEntryVariant() { Name = "TK3DES" };
                tk3desvar.KeyContainers.Add(new KeyVersion("Key Version A", 0, new Key(new string[] { "DES", KeyEntryClass.Symmetric.ToString() }, 24)));
                tk3desvar.KeyContainers.Add(new KeyVersion("Key Version B", 0, new Key(new string[] { "DES", KeyEntryClass.Symmetric.ToString() }, 24)));
                variants.Add(tk3desvar);
                var aes128var = new KeyEntryVariant() { Name = "AES128" };
                aes128var.KeyContainers.Add(new KeyVersion("Key Version A", 0, new Key(new string[] { "AES", KeyEntryClass.Symmetric.ToString() }, 16)));
                aes128var.KeyContainers.Add(new KeyVersion("Key Version B", 0, new Key(new string[] { "AES", KeyEntryClass.Symmetric.ToString() }, 16)));
                aes128var.KeyContainers.Add(new KeyVersion("Key Version C", 0, new Key(new string[] { "AES", KeyEntryClass.Symmetric.ToString() }, 16)));
                variants.Add(aes128var);
            }

            return variants;
        }
    }
}
