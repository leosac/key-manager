using Leosac.KeyManager.Library.KeyStore.Memory.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.Memory.UI
{
    public class MemoryKeyStoreUIFactory : KeyStoreUIFactory
    {
        public MemoryKeyStoreUIFactory()
        {
            targetFactory = new MemoryKeyStoreFactory();
        }

        public override string Name => "Memory Key Store";

        public override Type GetPropertiesType()
        {
            return typeof(MemoryKeyStoreProperties);
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
