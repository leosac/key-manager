using Leosac.KeyManager.Library.Plugin.UI.Domain;

namespace Leosac.KeyManager.Library.KeyStore.LCP.UI.Domain
{
    public class LCPKeyEntryPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public LCPKeyEntryPropertiesControlViewModel()
        {
            _properties = new LCPKeyEntryProperties();
        }

        public LCPKeyEntryProperties? LCPProperties
        {
            get { return Properties as LCPKeyEntryProperties; }
        }
    }
}
