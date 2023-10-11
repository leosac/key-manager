using Leosac.CredentialProvisioning.Core.Models;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.KeyStore.LCP.UI.Domain
{
    public class LCPKeyEntryPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public LCPKeyEntryPropertiesControlViewModel()
        {
            _properties = new LCPKeyEntryProperties();
            Scopes = new ObservableCollection<CredentialKeyScope>(Enum.GetValues<CredentialKeyScope>());
        }

        public LCPKeyEntryProperties? LCPProperties
        {
            get { return Properties as LCPKeyEntryProperties; }
        }

        public ObservableCollection<CredentialKeyScope> Scopes { get; private set; }
    }
}
