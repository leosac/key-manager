using Leosac.KeyManager.Library.Plugin.UI.Domain;

namespace Leosac.KeyManager.Library.KeyStore.LCP.UI.Domain
{
    public class LCPKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public LCPKeyStorePropertiesControlViewModel()
        {
            _properties = new LCPKeyStoreProperties();
        }

        public LCPKeyStoreProperties? LCPProperties
        {
            get { return Properties as LCPKeyStoreProperties; }
        }
    }
}
