using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.KeePass
{
    public class KeePassKeyStoreFactory : GenericKeyStoreFactory<KeePassKeyStore, KeePassKeyStoreProperties>
    {
        public override string Name => "KeePass Key Store";
    }
}
