using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.KeyStore;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class PublishKeyStoreDialogViewModel : ObservableValidator
    {
        public PublishKeyStoreDialogViewModel()
        {
            _options = new StoreOptions();
            _label = Properties.Resources.PublishKeyStore;
        }

        private string _label;
        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
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
