using Leosac.KeyManager.Library.Plugin.UI.Domain;

namespace Leosac.KeyManager.Library.KeyStore.KeePass.UI.Domain
{
    public class KeePassKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public KeePassKeyStorePropertiesControlViewModel()
        {
            _properties = new KeePassKeyStoreProperties();
        }

        public KeePassKeyStoreProperties? FileProperties
        {
            get { return Properties as KeePassKeyStoreProperties; }
        }
    }
}
