using Leosac.KeyManager.Library.KeyStore;

namespace Leosac.KeyManager.Library.Plugin.UI.Domain
{
    public abstract class KeyEntryPropertiesControlViewModel : ViewModelBase
    {
        protected KeyEntryProperties? _properties;

        public KeyEntryProperties? Properties
        {
            get => _properties;
            set => SetProperty(ref _properties, value);
        }
    }
}
