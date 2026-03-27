using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class PublishKeyStoreDialogViewModel : ObservableValidator
    {
        public PublishKeyStoreDialogViewModel()
        {
            _options = new StoreOptions();
            _label = Properties.Resources.PublishKeyStore;
            _batchOptions = new PublishBatchOptions();
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
            set
            {
                if (SetProperty(ref _favorite, value))
                    UpdateBatchSupport();
            }
        }

        private StoreOptions _options;
        public StoreOptions Options
        {
            get => _options;
            set => SetProperty(ref _options, value);
        }

        private PublishBatchOptions _batchOptions;
        public PublishBatchOptions BatchOptions
        {
            get => _batchOptions;
            set => SetProperty(ref _batchOptions, value);
        }

        private void UpdateBatchSupport()
        {
            if (Favorite?.Properties == null)
            {
                BatchOptions.IsSupported = false;
                return;
            }
            try
            {
                var factory = KeyStoreFactory.GetFactoryFromPropertyType(Favorite.Properties.GetType());
                if (factory == null)
                {
                    BatchOptions.IsSupported = false;
                    return;
                }
                bool supportsBatching;
                try
                {
                    var targetKs = factory.CreateKeyStore();
                    supportsBatching = targetKs?.SupportsBatching ?? false;
                }
                catch
                {
                    supportsBatching = false;
                }
                BatchOptions.IsSupported = supportsBatching;
            }
            catch
            {
                BatchOptions.IsSupported = false;
            }
        }

    }
}
