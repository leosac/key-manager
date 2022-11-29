using Net.Pkcs11Interop.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public class PKCS11KeyEntry : KeyEntry
    {
        public PKCS11KeyEntry()
        {
            Properties = new PKCS11KeyEntryProperties();
            Identifier.Id = Guid.NewGuid().ToString("N");
        }

        [JsonIgnore]
        public PKCS11KeyEntryProperties? PKCS11Properties
        {
            get { return Properties as PKCS11KeyEntryProperties; }
        }

        public override KeyEntryClass KClass
        {
            get => GetKeyEntryClassFromCKK(GetCKK());
        }

        public override IList<KeyEntryVariant> GetAllVariants()
        {
            var variants = new List<KeyEntryVariant>();

            var ckk = Enum.GetValues<CKK>();
            foreach(var k in ckk)
            {
                variants.Add(CreateVariantFromCKK(k));
            }

            return variants;
        }

        public static KeyEntryVariant CreateVariantFromCKK(CKK ckk)
        {
            var algo = ckk.ToString().Remove(0, 4);
            var variant = new KeyEntryVariant() { Name = algo };
            var tags = new string[]
            {
                    algo,
                    GetKeyEntryClassFromCKK(ckk).ToString()
            };
            variant.KeyVersions.Add(new KeyVersion("Key", 0, new Key(tags, KeyHelper.GetBlockSize(tags))));
            return variant;
        }

        public static KeyEntryClass GetKeyEntryClassFromCKK(CKK ckk)
        {
            if (ckk == CKK.CKK_DSA || ckk == CKK.CKK_ECDSA || ckk == CKK.CKK_RSA)
                return KeyEntryClass.Asymmetric;

            return KeyEntryClass.Symmetric;
        }

        public CKK GetCKK()
        {
            var ckk = CKK.CKK_GENERIC_SECRET;
            if (Variant != null && Variant.KeyVersions.Count > 0)
            {
                var tags = Variant.KeyVersions[0].Key.Tags;
                if (tags.Count > 0)
                {
                    Enum.TryParse<CKK>("CKK_" + tags[0], out ckk);
                }
            }

            return ckk;
        }
    }
}
