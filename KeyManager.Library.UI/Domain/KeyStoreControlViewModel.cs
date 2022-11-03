using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
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

            EditKeyEntryCommand = new KeyManagerAsyncCommand<string>(async
                keyEntryIdentifier =>
                {
                    var model = new KeyEntryDialogViewModel()
                    {
                        KeyEntry = KeyStore?.Get(keyEntryIdentifier),
                        CanChangeFactory = false
                    };
                    var dialog = new KeyEntryDialog()
                    {
                        DataContext = model
                    };
                    object? ret = await DialogHost.Show(dialog, "RootDialog");
                    if (ret != null && model.KeyEntry != null)
                    {
                        KeyStore?.Update(model.KeyEntry);
                    }
                });
        }

        private KeyStore.KeyStore? _keyStore;
        private string? _selectedKeyEntryIdentifier;
        private string? _searchTerm;

        public ObservableCollection<string> KeyEntryIdentifiers { get; }

        public KeyStore.KeyStore? KeyStore
        {
            get => _keyStore;
            set => SetProperty(ref _keyStore, value);
        }

        public string? SelectedKeyEntryIdentifier
        {
            get => _selectedKeyEntryIdentifier;
            set => SetProperty(ref _selectedKeyEntryIdentifier, value);
        }

        public string? SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        public KeyManagerAsyncCommand<string> EditKeyEntryCommand { get; }

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
