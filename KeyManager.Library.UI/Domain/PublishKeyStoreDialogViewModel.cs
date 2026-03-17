using System.Diagnostics;
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
            try
            {
                if (Favorite?.Properties == null)
                {
                    BatchOptions.IsSupported = false;
                    return;
                }
                var factory = KeyStoreFactory.GetFactoryFromPropertyType(Favorite.Properties.GetType())
                              ?? throw new KeyStoreException("No factory found for Favorite Properties type.");
                var targetKs = factory.CreateKeyStore()
                               ?? throw new KeyStoreException("Failed to create KeyStore from Favorite.");
                BatchOptions.IsSupported = targetKs.SupportsBatching;
            }
            catch (Exception ex)
            {
                BatchOptions.IsSupported = false;
                throw new KeyStoreException("Unable to determine batch support from Favorite.", ex);
            }
        }

    }
}
