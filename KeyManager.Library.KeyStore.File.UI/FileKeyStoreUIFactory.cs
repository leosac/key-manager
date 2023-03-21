using Leosac.KeyManager.Library.KeyStore.File.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.File.UI
{
    public class FileKeyStoreUIFactory : KeyStoreUIFactory
    {
        public FileKeyStoreUIFactory()
        {
            targetFactory = new FileKeyStoreFactory();
        }

        public override string Name => "File Key Store";

        public override Type GetPropertiesType()
        {
            return typeof(FileKeyStoreProperties);
        }

        public override UserControl CreateKeyStorePropertiesControl()
        {
            return new FileKeyStorePropertiesControl();
        }

        public override KeyStorePropertiesControlViewModel CreateKeyStorePropertiesControlViewModel()
        {
            return new FileKeyStorePropertiesControlViewModel();
        }

        public override IDictionary<string, UserControl> CreateKeyStoreAdditionalControls()
        {
            return new Dictionary<string, UserControl>();
        }
    }
}
