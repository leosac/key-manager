using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.CNG
{
    public class CNGKeyStoreFactory : GenericKeyStoreFactory<CNGKeyStore, CNGKeyStoreProperties>
    {
        public override string Name => "Cryptography API: Next Generation (CNG)";
    }
}
