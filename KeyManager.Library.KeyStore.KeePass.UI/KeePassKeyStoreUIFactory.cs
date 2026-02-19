using KeyManager.Library.KeyStore.KeePass.UI;
using Leosac.KeyManager.Library.KeyStore.KeePass.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.KeePass.UI
{
    public class KeePassKeyStoreUIFactory : KeyStoreUIFactory
    {
        public KeePassKeyStoreUIFactory()
        {
            targetFactory = new KeePassKeyStoreFactory();
        }

        public override string Name => "KeePass Key Store";

        public override Type? GetPropertiesType()
        {
            return typeof(KeePassKeyStoreProperties);
        }
        public override UserControl CreateKeyStorePropertiesControl()
        {
            return new KeePassKeyStorePropertiesControl();
        }

        public override KeyStorePropertiesControlViewModel? CreateKeyStorePropertiesControlViewModel()
        {
            return new KeePassKeyStorePropertiesControlViewModel();
        }

        public override IDictionary<string, UserControl> CreateKeyStoreAdditionalControls()
        {
            return new Dictionary<string, UserControl>();
        }
    }
}
