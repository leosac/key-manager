using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public abstract class PKCS11KeyEntry : KeyEntry
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

        public override IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter = null)
        {
            var variants = new List<KeyEntryVariant>();

            IEnumerable<CKK> ckk;
            var asymckk = GetAsymmetricCKKs();
            if (classFilter == KeyEntryClass.Asymmetric)
            {
                ckk = asymckk;
            }
            else
            {
                ckk = Enum.GetValues<CKK>();
                ckk = ckk.Except(asymckk);
            }

            foreach(var k in ckk)
            {
                variants.Add(CreateVariantFromCKK(k));
            }

            return variants;
        }

        public IEnumerable<CKK> GetAsymmetricCKKs()
        {
            return new CKK[]
            {
                CKK.CKK_RSA,
                CKK.CKK_DSA,
                CKK.CKK_ECDSA
            };
        }

        public KeyEntryVariant CreateVariantFromCKK(CKK ckk)
        {
            var algo = ckk.ToString().Remove(0, 4);
            var variant = new KeyEntryVariant() { Name = algo };
            var tags = new string[]
            {
                    algo,
                    KClass.ToString()
            };
            variant.KeyContainers.Add(new KeyContainer("Key", new Key(tags, KeyHelper.GetBlockSize(tags))));
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
            return GetCKK(this);
        }

        public static CKK GetCKK(KeyEntry entry)
        {
            var ckk = CKK.CKK_GENERIC_SECRET;
            if (entry.Variant != null && entry.Variant.KeyContainers.Count > 0)
            {
                var tags = entry.Variant.KeyContainers[0].Key.Tags;
                if (tags.Count > 0)
                {
                    Enum.TryParse<CKK>("CKK_" + tags[0], out ckk);
                }
            }

            return ckk;
        }

        public abstract void GetAttributes(ISession session, IObjectHandle? handle);
    }
}
