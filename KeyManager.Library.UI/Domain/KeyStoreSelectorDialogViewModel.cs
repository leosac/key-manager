using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyStoreSelectorDialogViewModel : ViewModelBase
    {
        public KeyStoreSelectorDialogViewModel()
        {
            KeyStoreFactories = new ObservableCollection<KeyStoreItem>();
            foreach (var factory in KeyStoreFactory.RegisteredFactories)
            {
                KeyStoreFactories.Add(new KeyStoreItem(factory));
            }
        }   

        private KeyStoreItem? _selectedFactoryItem;
        private int _selectedFactoryIndex;
        private KeyStoreProperties? _keyStoreProperties;
        private string? _message;

        public ObservableCollection<KeyStoreItem> KeyStoreFactories { get; }

        public KeyStoreItem? SelectedFactoryItem
        {
            get => _selectedFactoryItem;
            set => SetProperty(ref _selectedFactoryItem, value);
        }

        public int SelectedFactoryIndex
        {
            get => _selectedFactoryIndex;
            set => SetProperty(ref _selectedFactoryIndex, value);
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
