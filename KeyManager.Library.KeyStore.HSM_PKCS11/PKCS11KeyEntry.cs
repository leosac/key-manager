using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public abstract class PKCS11KeyEntry : KeyEntry
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        protected PKCS11KeyEntry()
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
            return
            [
                CKK.CKK_RSA,
                CKK.CKK_DSA,
                CKK.CKK_ECDSA
            ];
        }

        public KeyEntryVariant CreateVariantFromCKK(CKK ckk)
        {
            var algo = ckk.ToString().Remove(0, 4);
            return CreateVariantFromAlgo<KeyContainer>(algo, GetKeySize(ckk));
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
            {
                return KeyEntryClass.Asymmetric;
            }

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

        public virtual void GetAttributes(ISession session, IObjectHandle? handle)
        {
            var attributes = session!.GetAttributeValue(handle, new List<CKA>
            {
                CKA.CKA_ENCRYPT,
                CKA.CKA_DECRYPT,
                CKA.CKA_DERIVE,
                CKA.CKA_EXTRACTABLE,
                CKA.CKA_SENSITIVE
            });

            foreach (var attribute in attributes)
            {
                if (attribute.CannotBeRead)
                {
                    log.Warn(string.Format("The attribute `{0}` ({1}) cannot be read.", attribute.Type, (CKA)attribute.Type));
                }
                switch (attribute.Type)
                {
                    case (ulong)CKA.CKA_ENCRYPT:
                        PKCS11Properties!.Encrypt = (!attribute.CannotBeRead) ? attribute.GetValueAsBool() : null;
                        break;
                    case (ulong)CKA.CKA_DECRYPT:
                        PKCS11Properties!.Decrypt = (!attribute.CannotBeRead) ? attribute.GetValueAsBool() : null;
                        break;
                    case (ulong)CKA.CKA_DERIVE:
                        PKCS11Properties!.Derive = (!attribute.CannotBeRead) ? attribute.GetValueAsBool() : null;
                        break;
                    case (ulong)CKA.CKA_EXTRACTABLE:
                        PKCS11Properties!.Extractable = (!attribute.CannotBeRead) ? attribute.GetValueAsBool() : null;
                        break;
                    case (ulong)CKA.CKA_SENSITIVE:
                        PKCS11Properties!.Sensitive = (!attribute.CannotBeRead) ? attribute.GetValueAsBool() : null;
                        break;
                    default:
                        throw new KeyStoreException("Unexpected attribute.");
                }
            }
        }
    }
}
