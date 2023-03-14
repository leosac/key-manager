using Leosac.KeyManager.Library.KeyStore.Memory.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.Memory.UI
{
    public class MemoryKeyStoreFactory : KeyStoreFactory
    {
        public override string Name => "Memory Key Store";

        public override KeyStore CreateKeyStore()
        {
            return new MemoryKeyStore();
        }

        public override Type GetKeyStorePropertiesType()
        {
            return typeof(MemoryKeyStoreProperties);
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new MemoryKeyStoreProperties();
        }

        public override UserControl CreateKeyStorePropertiesControl()
        {
            return new MemoryKeyStorePropertiesControl();
        }

        public override KeyStorePropertiesControlViewModel CreateKeyStorePropertiesControlViewModel()
        {
            return new MemoryKeyStorePropertiesControlViewModel();
        }

        public override IDictionary<string, UserControl> CreateKeyStoreAdditionalControls()
        {
            return new Dictionary<string, UserControl>();
        }
    }
}
