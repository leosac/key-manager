using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using Leosac.WpfApp.Domain;
using MaterialDesignThemes.Wpf;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public abstract class KeyStoreAdditionalControlViewModel : ObservableValidator
    {
        private KeyStore.KeyStore? _keyStore;

        public virtual KeyStore.KeyStore? KeyStore
        {
            get => _keyStore;
            set => SetProperty(ref _keyStore, value);
        }

        public ISnackbarMessageQueue? SnackbarMessageQueue { get; set; }
    }
}
