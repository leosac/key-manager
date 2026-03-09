using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.LCP
{
    public class LCPKeyStoreFactory : GenericKeyStoreFactory<LCPKeyStore, LCPKeyStoreProperties>
    {
        public override string Name => "Leosac Credential Provisioning Server";
    }
}
