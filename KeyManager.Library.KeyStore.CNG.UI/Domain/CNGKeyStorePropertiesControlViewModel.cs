using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.KeyStore.CNG.UI.Domain
{
    public class CNGKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public CNGKeyStorePropertiesControlViewModel()
        {
            _properties = new CNGKeyStoreProperties();
            StorageProviders = [.. CNGKeyStore.GetExistingProviders()];
            Scopes = [.. Enum.GetValues<CNGScope>()];
        }

        public CNGKeyStoreProperties? CNGProperties
        {
            get { return Properties as CNGKeyStoreProperties; }
        }

        public ObservableCollection<string> StorageProviders { get; private set; }

        public ObservableCollection<CNGScope> Scopes { get; private set; }
    }
}
