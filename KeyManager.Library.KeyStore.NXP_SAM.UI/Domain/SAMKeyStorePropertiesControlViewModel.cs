using Leosac.KeyManager.Library.Plugin.UI.Domain;
using LibLogicalAccess.Card;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain
{
    public class SAMKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public SAMKeyStorePropertiesControlViewModel()
        {
            _properties = new SAMKeyStoreProperties();
            KeyTypes = new ObservableCollection<DESFireKeyType>(Enum.GetValues<DESFireKeyType>());
            CardTypes = new ObservableCollection<string>(new[]
            {
                "SAM_AV1",
                "SAM_AV2",
                "SAM_AV3"
            });
            AuthenticationModes = new ObservableCollection<SAMAuthenticationMode>(Enum.GetValues<SAMAuthenticationMode>());
        }

        public SAMKeyStoreProperties? SAMProperties
        {
            get { return Properties as SAMKeyStoreProperties; }
        }

        public ObservableCollection<DESFireKeyType> KeyTypes { get; set; }

        public ObservableCollection<string> CardTypes { get; set; }

        public ObservableCollection<SAMAuthenticationMode> AuthenticationModes { get; set; }
    }
}
