using Leosac.KeyManager.Library.Plugin.Domain;

namespace Leosac.KeyManager.Library.KeyStore.Memory.UI.Domain
{
    public class MemoryKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public MemoryKeyStorePropertiesControlViewModel()
        {
            _properties = new MemoryKeyStoreProperties();
        }
    }
}
