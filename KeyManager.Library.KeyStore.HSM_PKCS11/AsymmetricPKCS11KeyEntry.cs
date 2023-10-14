using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public class AsymmetricPKCS11KeyEntry : PKCS11KeyEntry
    {
        public AsymmetricPKCS11KeyEntry() : this(KeyEntryClass.Asymmetric)
        {

        }

        public AsymmetricPKCS11KeyEntry(KeyEntryClass kclass)
        {
            Properties = new AsymmetricPKCS11KeyEntryProperties();
            _kclass = kclass;
        }

        private readonly KeyEntryClass _kclass;

        public override KeyEntryClass KClass => _kclass;

        public override void GetAttributes(ISession session, IObjectHandle? handle)
        {
            var attributes = session!.GetAttributeValue(handle, new List<CKA>
            {
                CKA.CKA_ENCRYPT,
                CKA.CKA_DECRYPT,
                CKA.CKA_DERIVE,
                CKA.CKA_EXTRACTABLE
            });

            foreach (var attribute in attributes)
            {
                switch (attribute.Type)
                {
                    case (ulong)CKA.CKA_ENCRYPT:
                        PKCS11Properties!.Encrypt = attribute.GetValueAsBool();
                        break;
                    case (ulong)CKA.CKA_DECRYPT:
                        PKCS11Properties!.Decrypt = attribute.GetValueAsBool();
                        break;
                    case (ulong)CKA.CKA_DERIVE:
                        PKCS11Properties!.Derive = attribute.GetValueAsBool();
                        break;
                    case (ulong)CKA.CKA_EXTRACTABLE:
                        PKCS11Properties!.Extractable = attribute.GetValueAsBool();
                        break;
                    default:
                        throw new KeyStoreException("Unexpected attribute.");
                }
            }
        }
    }
}
