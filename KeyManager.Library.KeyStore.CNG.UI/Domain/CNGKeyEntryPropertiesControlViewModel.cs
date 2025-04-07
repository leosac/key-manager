using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.KeyStore.CNG.UI.Domain
{
    public class CNGKeyEntryPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public CNGKeyEntryPropertiesControlViewModel()
        {
            _properties = new CNGKeyEntryProperties();
        }

        public CNGKeyEntryProperties? CNGProperties
        {
            get { return Properties as CNGKeyEntryProperties; }
        }
    }
}
