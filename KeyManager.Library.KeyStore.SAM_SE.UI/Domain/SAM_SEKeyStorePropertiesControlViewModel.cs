using Leosac.KeyManager.Library.Plugin.UI.Domain;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain
{
    public class SAM_SEKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public SAM_SEKeyStorePropertiesControlViewModel()
        {
            _properties = new SAM_SEKeyStoreProperties();
        }

        public SAM_SEKeyStoreProperties? SAM_SEProperties
        {
            get { return Properties as SAM_SEKeyStoreProperties; }
        }
    }
}
