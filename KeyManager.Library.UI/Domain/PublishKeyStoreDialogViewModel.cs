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
            _wrappingKeySelector = "0";
            _wrappingKeyId = new KeyEntryId();
        }

        private Favorite? _favorite;

        public Favorite? Favorite
        {
            get => _favorite;
            set => SetProperty(ref _favorite, value);
        }

        private KeyEntryId _wrappingKeyId;

        public KeyEntryId WrappingKeyId
        {
            get => _wrappingKeyId;
            set => SetProperty(ref _wrappingKeyId, value);
        }

        private string _wrappingKeySelector;

        public string WrappingKeySelector
        {
            get => _wrappingKeySelector;
            set => SetProperty(ref _wrappingKeySelector, value);
        }
    }
}
