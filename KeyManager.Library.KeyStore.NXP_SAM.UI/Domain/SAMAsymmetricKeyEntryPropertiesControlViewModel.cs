using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain
{
    public class SAMAsymmetricKeyEntryPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public SAMAsymmetricKeyEntryPropertiesControlViewModel()
        {
            _properties = new SAMAsymmetricKeyEntryProperties();
        }

        public SAMAsymmetricKeyEntryProperties? SAMProperties
        {
            get { return Properties as SAMAsymmetricKeyEntryProperties; }
        }
    }
}
