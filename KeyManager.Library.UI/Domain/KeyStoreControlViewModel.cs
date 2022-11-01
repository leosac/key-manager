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
            KeyEntryIdentifiers = new ObservableCollection<string>();
        }

        private KeyStore.KeyStore? _keyStore;
        private KeyStore.KeyEntry? _selectedKeyEntry;
        private string? _searchTerm;

        public ObservableCollection<string> KeyEntryIdentifiers { get; }

        public KeyStore.KeyStore? KeyStore
        {
            get => _keyStore;
            set => SetProperty(ref _keyStore, value);
        }

        public KeyStore.KeyEntry? SelectedKeyEntry
        {
            get => _selectedKeyEntry;
            set => SetProperty(ref _selectedKeyEntry, value);
        }

        public string? SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        public void RefreshKeyEntries()
        {
            KeyEntryIdentifiers.Clear();
            if (KeyStore != null)
            {
                foreach (var id in KeyStore.GetAll())
                {
                    KeyEntryIdentifiers.Add(id);
                }
            }
        }
    }
}
