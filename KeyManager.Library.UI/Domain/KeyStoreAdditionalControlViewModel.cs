using Leosac.KeyManager.Library.Plugin.Domain;
using MaterialDesignThemes.Wpf;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public abstract class KeyStoreAdditionalControlViewModel : ViewModelBase
    {
        private KeyStore.KeyStore? _keyStore;

        public KeyStore.KeyStore? KeyStore
        {
            get => _keyStore;
            set => SetProperty(ref _keyStore, value);
        }

        public ISnackbarMessageQueue? SnackbarMessageQueue { get; set; }
    }
}
