using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.Memory.UI
{
    public class MemoryKeyStoreFactory : GenericKeyStoreFactory<MemoryKeyStore, MemoryKeyStoreProperties>
    {
        public override string Name => "Memory Key Store";
    }
}
