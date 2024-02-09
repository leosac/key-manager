using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using Leosac.WpfApp.Domain;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class PublishKeyStoreDialogViewModel : ObservableValidator
    {
        public PublishKeyStoreDialogViewModel()
        {
            _options = new StoreOptions();
        }

        private Favorite? _favorite;
        public Favorite? Favorite
        {
            get => _favorite;
            set => SetProperty(ref _favorite, value);
        }

        private StoreOptions _options;
        public StoreOptions Options
        {
            get => _options;
            set => SetProperty(ref _options, value);
        }
    }
}
