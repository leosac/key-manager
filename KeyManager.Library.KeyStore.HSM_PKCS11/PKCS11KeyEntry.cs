using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public abstract class PKCS11KeyEntry : KeyEntry
    {
        public PKCS11KeyEntry()
        {
            Identifier.Id = Guid.NewGuid().ToString("N");
        }

        [JsonIgnore]
        public PKCS11KeyEntryProperties? PKCS11Properties
        {
            get { return Properties as PKCS11KeyEntryProperties; }
        }

        public override IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter)
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

        public static IEnumerable<CKK> GetAsymmetricCKKs()
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
            return CreateVariantFromAlgo(algo, GetKeySize(ckk));
        }

        private static uint GetKeySize(CKK ckk)
        {
            uint size = 0;
            if (ckk == CKK.CKK_DES)
            {
                size = 8;
            }
            else if (ckk == CKK.CKK_DES2 || ckk == CKK.CKK_AES)
            {
                size = 16;
            }
            else if (ckk == CKK.CKK_DES3)
            {
                size = 24;
            }
            return size;
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
                    if (!Enum.TryParse("CKK_" + tags[0], out ckk))
                    {
                        ckk = CKK.CKK_GENERIC_SECRET;
                    }
                }
            }

            return ckk;
        }

        public abstract void GetAttributes(ISession session, IObjectHandle? handle);
    }
}
