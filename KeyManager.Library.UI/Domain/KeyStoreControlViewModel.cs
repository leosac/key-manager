using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyStoreControlViewModel : ViewModelBase
    {
        public KeyStoreControlViewModel()
        {
            KeyEntryFactories = new ObservableCollection<KeyEntryItem>();
            foreach (var factory in KeyEntryFactory.RegisteredFactories)
            {
                KeyEntryFactories.Add(new KeyEntryItem(factory));
            }
        }

        private KeyStore.KeyStore? _keyStore;
        private KeyEntryItem? _selectedFactoryItem;
        private int _selectedFactoryIndex;
        private string _searchTerm;

        public ObservableCollection<KeyEntryItem> KeyEntryFactories { get; }

        public KeyStore.KeyStore? KeyStore
        {
            get => _keyStore;
            set => SetProperty(ref _keyStore, value);
        }

        public KeyEntryItem? SelectedFactoryItem
        {
            get => _selectedFactoryItem;
            set => SetProperty(ref _selectedFactoryItem, value);
        }

        public int SelectedFactoryIndex
        {
            get => _selectedFactoryIndex;
            set => SetProperty(ref _selectedFactoryIndex, value);
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }
    }
}
