using LibLogicalAccess.Card;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMSymmetricKeyEntry : SymmetricKeyEntry
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

        public override IList<KeyEntryVariant> GetAllVariants()
        {
            var variants = new List<KeyEntryVariant>();

            var desvar = new KeyEntryVariant() { Name = "DES" };
            desvar.KeyVersions.Add(new KeyVersion("Key Version A", 0, new Key(new string[] { "DES" }, 16)));
            desvar.KeyVersions.Add(new KeyVersion("Key Version B", 0, new Key(new string[] { "DES" }, 16)));
            desvar.KeyVersions.Add(new KeyVersion("Key Version C", 0, new Key(new string[] { "DES" }, 16)));
            variants.Add(desvar);
            var tk3desvar = new KeyEntryVariant() { Name = "TK3DES" };
            tk3desvar.KeyVersions.Add(new KeyVersion("Key Version A", 0, new Key(new string[] { "DES" }, 24)));
            tk3desvar.KeyVersions.Add(new KeyVersion("Key Version B", 0, new Key(new string[] { "DES" }, 24)));
            variants.Add(tk3desvar);
            var aes128var = new KeyEntryVariant() { Name = "AES128" };
            aes128var.KeyVersions.Add(new KeyVersion("Key Version A", 0, new Key(new string[] { "AES" }, 16)));
            aes128var.KeyVersions.Add(new KeyVersion("Key Version B", 0, new Key(new string[] { "AES" }, 16)));
            aes128var.KeyVersions.Add(new KeyVersion("Key Version C", 0, new Key(new string[] { "AES" }, 16)));
            variants.Add(aes128var);

            return variants;
        }
    }
}
