using Leosac.KeyManager.Library.KeyStore;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyStoreControlViewModel : ViewModelBase
    {
        public KeyStoreControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            KeyEntryIdentifiers = new ObservableCollection<string>();

            EditKeyEntryCommand = new KeyManagerAsyncCommand<string>(async
                keyEntryIdentifier =>
                {
                    try
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
                    }
                    catch (KeyStoreException ex)
                    {
                        snackbarMessageQueue.Enqueue(String.Format("Key Store Error: {0}", ex.Message), new PackIcon { Kind = PackIconKind.CloseBold }, (object? p) => { }, null, false, true, TimeSpan.FromSeconds(5));
                    }
                    catch (Exception ex)
                    {
                        snackbarMessageQueue.Enqueue(ex.Message, new PackIcon { Kind = PackIconKind.CloseBold }, (object? p) => { }, null, false, true, TimeSpan.FromSeconds(5));
                    }
                });

            _keyEntryIdentifiersView = CollectionViewSource.GetDefaultView(KeyEntryIdentifiers);
            _keyEntryIdentifiersView.Filter = KeyEntryIdentifiersFilter;
        }

        private KeyStore.KeyStore? _keyStore;
        private readonly ICollectionView _keyEntryIdentifiersView;
        private string? _selectedKeyEntryIdentifier;
        private string? _searchTerms;

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

        public string? SearchTerms
        {
            get => _searchTerms;
            set
            {
                if (SetProperty(ref _searchTerms, value))
                {
                    RefreshKeyEntriesView();
                }
            }
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

        public void RefreshKeyEntriesView()
        {
            _keyEntryIdentifiersView.Refresh();
        }

        private bool KeyEntryIdentifiersFilter(object obj)
        {
            if (string.IsNullOrWhiteSpace(_searchTerms))
            {
                return true;
            }

            return obj is string item
                   && item.ToLower().Contains(_searchTerms!.ToLower());
        }
    }
}
