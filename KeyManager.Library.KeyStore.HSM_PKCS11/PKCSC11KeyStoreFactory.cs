using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public class PKCSC11KeyStoreFactory : GenericKeyStoreFactory<PKCS11KeyStore, PKCS11KeyStoreProperties>
    {
        public override string Name => "HSM PKCS#11";
    }
}
