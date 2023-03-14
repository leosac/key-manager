using Leosac.KeyManager.Library.Plugin.Domain;

namespace Leosac.KeyManager.Library.KeyStore.File.UI.Domain
{
    public class FileKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public FileKeyStorePropertiesControlViewModel()
        {
            _properties = new FileKeyStoreProperties();
        }

        public FileKeyStoreProperties? FileProperties
        {
            get { return Properties as FileKeyStoreProperties; }
        }
    }
}
