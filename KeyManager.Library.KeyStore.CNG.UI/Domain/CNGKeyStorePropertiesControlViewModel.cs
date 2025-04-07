using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.KeyStore.CNG.UI.Domain
{
    public class CNGKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public CNGKeyStorePropertiesControlViewModel()
        {
            _properties = new CNGKeyStoreProperties();
            StorageProviders = new ObservableCollection<string>(["", "Microsoft Software Key Storage Provider", "Microsoft Smart Card Key Storage Provider", "Microsoft Platform Crypto Provider"]);
        }

        public CNGKeyStoreProperties? CNGProperties
        {
            get { return Properties as CNGKeyStoreProperties; }
        }

        public ObservableCollection<string> StorageProviders { get; private set; }
    }
}
