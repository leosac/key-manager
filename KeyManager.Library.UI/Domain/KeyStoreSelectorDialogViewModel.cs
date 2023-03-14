using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.Domain;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyStoreSelectorDialogViewModel : ViewModelBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public KeyStoreSelectorDialogViewModel()
        {
            KeyStoreFactories = new ObservableCollection<KeyStoreItem>();
            foreach (var factory in KeyStoreFactory.RegisteredFactories)
            {
                try
                {
                    KeyStoreFactories.Add(new KeyStoreItem(factory));
                }
                catch(Exception ex)
                {
                    log.Error("Key Store factory error.", ex);
                }
            }
        }   

        private KeyStoreItem? _selectedFactoryItem;
        private string? _message;

        public ObservableCollection<KeyStoreItem> KeyStoreFactories { get; }

        public KeyStoreItem? SelectedFactoryItem
        {
            get => _selectedFactoryItem;
            set => SetProperty(ref _selectedFactoryItem, value);
        }

        public string? Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public KeyStore.KeyStore? CreateKeyStore()
        {
            KeyStore.KeyStore? store = null;
            if (SelectedFactoryItem != null)
            {
                store = SelectedFactoryItem.Factory.CreateKeyStore();
                store.Properties = SelectedFactoryItem.DataContext?.Properties;
            }
            return store;
        }
    }
}
