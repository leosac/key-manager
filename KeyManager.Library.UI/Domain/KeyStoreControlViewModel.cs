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
            _snackbarMessageQueue = snackbarMessageQueue;
            KeyEntryIdentifiers = new ObservableCollection<string>();

            CreateKeyEntryCommand = new KeyManagerAsyncCommand<string>(async
                keyEntryIdentifier =>
            {
                var model = new KeyEntryDialogViewModel();
                var dialog = new KeyEntryDialog()
                {
                    DataContext = model
                };
                CreateKeyEntry(dialog);
            });

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

                UpdateKeyEntry(dialog);
            });

            DeleteKeyEntryCommand = new KeyManagerAsyncCommand<string>(async
                keyEntryIdentifier =>
            {
                DeleteKeyEntry(keyEntryIdentifier);
            });

            _keyEntryIdentifiersView = CollectionViewSource.GetDefaultView(KeyEntryIdentifiers);
            _keyEntryIdentifiersView.Filter = KeyEntryIdentifiersFilter;
        }

        private ISnackbarMessageQueue _snackbarMessageQueue;
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

        public KeyManagerAsyncCommand<string> CreateKeyEntryCommand { get; }

        private async void CreateKeyEntry(KeyEntryDialog dialog)
        {
            object? ret = await DialogHost.Show(dialog, "KeyStoreDialog");
            if (ret != null && dialog.DataContext is KeyEntryDialogViewModel model)
            {
                if (model.KeyEntry != null)
                {
                    try
                    {
                        KeyStore?.Create(model.KeyEntry);
                        KeyEntryIdentifiers.Add(model.KeyEntry.Identifier);
                    }
                    catch (KeyStoreException ex)
                    {
                        SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
                        CreateKeyEntry(dialog);
                    }
                    catch (Exception ex)
                    {
                        SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
                        CreateKeyEntry(dialog);
                    }
                }
            }
        }

        public KeyManagerAsyncCommand<string> EditKeyEntryCommand { get; }

        private async void UpdateKeyEntry(KeyEntryDialog dialog)
        {
            try
            {
                object? ret = await DialogHost.Show(dialog, "KeyStoreDialog");
                if (ret != null && dialog.DataContext is KeyEntryDialogViewModel model)
                {
                    if (model.KeyEntry != null)
                    {
                        KeyStore?.Update(model.KeyEntry);
                    }
                }
            }
            catch (KeyStoreException ex)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
                UpdateKeyEntry(dialog);
            }
            catch (Exception ex)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
                UpdateKeyEntry(dialog);
            }
        }

        public KeyManagerAsyncCommand<string> DeleteKeyEntryCommand { get; }

        private void DeleteKeyEntry(string identifier)
        {
            try
            {
                KeyStore?.Delete(identifier);
                KeyEntryIdentifiers.Remove(identifier);
            }
            catch (KeyStoreException ex)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
            }
            catch (Exception ex)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
            }
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
