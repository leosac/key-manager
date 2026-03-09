using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.File.UI
{
    public class FileKeyStoreFactory : GenericKeyStoreFactory<FileKeyStore, FileKeyStoreProperties>
    {
        public override string Name => "File Key Store";
    }
}
