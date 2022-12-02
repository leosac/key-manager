using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public class AsymmetricPKCS11KeyEntry : PKCS11KeyEntry
    {
        public AsymmetricPKCS11KeyEntry(KeyEntryClass kclass = KeyEntryClass.Asymmetric)
        {
            _kclass = kclass;
        }

        KeyEntryClass _kclass;

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
                if (attribute.Type == (ulong)CKA.CKA_ENCRYPT)
                    PKCS11Properties!.Encrypt = attribute.GetValueAsBool();
                else if (attribute.Type == (ulong)CKA.CKA_DECRYPT)
                    PKCS11Properties!.Decrypt = attribute.GetValueAsBool();
                else if (attribute.Type == (ulong)CKA.CKA_DERIVE)
                    PKCS11Properties!.Derive = attribute.GetValueAsBool();
                else if (attribute.Type == (ulong)CKA.CKA_EXTRACTABLE)
                    PKCS11Properties!.Extractable = attribute.GetValueAsBool();
                else
                    throw new KeyStoreException("Unexpected attribute.");
            }
        }
    }
}
