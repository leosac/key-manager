using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Leosac.KeyManager.Library.Plugin
{
    public abstract class KeyStoreUIFactory : KMFactory<KeyStoreUIFactory>
    {
        protected KeyStoreFactory? targetFactory;

        public abstract UserControl CreateKeyStorePropertiesControl();

        public abstract KeyStorePropertiesControlViewModel CreateKeyStorePropertiesControlViewModel();

        public abstract IDictionary<string, UserControl> CreateKeyStoreAdditionalControls();

        public KeyStoreFactory? TargetFactory => targetFactory;
    }
}
