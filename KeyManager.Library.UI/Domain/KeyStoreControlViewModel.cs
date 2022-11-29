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
using static Net.Codecrete.QrCodeGenerator.QrSegment;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyStoreControlViewModel : ViewModelBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        public KeyStoreControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _snackbarMessageQueue = snackbarMessageQueue;
            Identifiers = new ObservableCollection<KeyEntryId>();

            CreateKeyEntryCommand = new KeyManagerAsyncCommand<string>(async
                parameter =>
            {
                var model = new KeyEntryDialogViewModel();
                var dialog = new KeyEntryDialog()
                {
                    DataContext = model
                };
                CreateKeyEntry(dialog);
            });

            EditKeyEntryCommand = new KeyManagerAsyncCommand<KeyEntryId>(async
                keyEntryIdentifier =>
            {
                var model = new KeyEntryDialogViewModel()
                {
                    KeyEntry = KeyStore?.Get(keyEntryIdentifier, _keClass),
                    CanChangeFactory = false
                };
                var factory = KeyEntryFactory.GetFactoryFromPropertyType(model.KeyEntry!.Properties?.GetType());
                if (factory != null)
                {
                    model.AutoCreate = false;
                    model.SelectedFactoryItem = model.KeyEntryFactories.Where(item => item.Factory == factory).FirstOrDefault();
                    model.SelectedFactoryItem!.DataContext!.Properties = model.KeyEntry.Properties;
                    var variant = model.KeyEntry.Variant;
                    if (variant != null)
                    {
                        model.RefreshVariants();
                        var emptyv = model.Variants.Where(v => v.Name == variant.Name).FirstOrDefault();
                        if (emptyv != null)
                        {
                            model.Variants.Remove(emptyv);
                        }
                        model.Variants.Add(variant);
                        model.KeyEntry.Variant = variant;
                    }
                }
                var dialog = new KeyEntryDialog()
                {
                    DataContext = model
                };

                UpdateKeyEntry(dialog);
            });

            DeleteKeyEntryCommand = new KeyManagerAsyncCommand<KeyEntryId>(async
                keyEntryIdentifier =>
            {
                DeleteKeyEntry(keyEntryIdentifier);
            });

            ImportCryptogramCommand = new KeyManagerAsyncCommand<KeyEntryId?>(async
                parameter =>
            {
                var model = new ImportCryptogramDialogViewModel()
                {
                    CanChangeIdentifier = parameter == null || !parameter.IsConfigured()
                };
                if (parameter != null)
                {
                    model.Cryptogram.Identifier = parameter;
                }
                var dialog = new ImportCryptogramDialog()
                {
                    DataContext = model,
                };
                ImportCryptogram(dialog);
            });

            _keClass = KeyEntryClass.Symmetric;
            _identifiersView = CollectionViewSource.GetDefaultView(Identifiers);
            _identifiersView.Filter = KeyEntryIdentifiersFilter;
        }

        protected ISnackbarMessageQueue _snackbarMessageQueue;
        private KeyStore.KeyStore? _keyStore;
        private KeyEntryClass _keClass;
        private readonly ICollectionView _identifiersView;
        private KeyEntryId? _selectedIdentifier;
        private string? _searchTerms;

        public ObservableCollection<KeyEntryId> Identifiers { get; }

        public KeyStore.KeyStore? KeyStore
        {
            get => _keyStore;
            set => SetProperty(ref _keyStore, value);
        }

        public KeyEntryClass KeyEntryClass
        {
            get => _keClass;
            set => SetProperty(ref _keClass, value);
        }

        public KeyEntryId? SelectedIdentifier
        {
            get => _selectedIdentifier;
            set => SetProperty(ref _selectedIdentifier, value);
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
                        Identifiers.Add(model.KeyEntry.Identifier);
                    }
                    catch (KeyStoreException ex)
                    {
                        SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
                        CreateKeyEntry(dialog);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Creating the Key Entry failed unexpected.", ex);
                        SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
                        CreateKeyEntry(dialog);
                    }
                }
            }
        }

        public KeyManagerAsyncCommand<KeyEntryId> EditKeyEntryCommand { get; }

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
                log.Error("Updating the Key Entry failed unexpected.", ex);
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
                UpdateKeyEntry(dialog);
            }
        }

        public KeyManagerAsyncCommand<KeyEntryId> DeleteKeyEntryCommand { get; }

        private void DeleteKeyEntry(KeyEntryId identifier)
        {
            try
            {
                KeyStore?.Delete(identifier, _keClass);
                Identifiers.Remove(identifier);
            }
            catch (KeyStoreException ex)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
            }
            catch (Exception ex)
            {
                log.Error("Deleting the Key Entry failed unexpected.", ex);
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
            }
        }

        public KeyManagerAsyncCommand<KeyEntryId?> ImportCryptogramCommand { get; }

        private async void ImportCryptogram(ImportCryptogramDialog dialog)
        {
            try
            {
                object? ret = await DialogHost.Show(dialog, "KeyStoreDialog");
                if (ret != null && dialog.DataContext is ImportCryptogramDialogViewModel model)
                {
                    if (!string.IsNullOrEmpty(model.Cryptogram.Value))
                    {
                        KeyStore?.Update(model.Cryptogram);
                    }
                }
            }
            catch (KeyStoreException ex)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
                ImportCryptogram(dialog);
            }
            catch (Exception ex)
            {
                log.Error("Importing the Key Entry Cryptogram failed unexpected.", ex);
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
                ImportCryptogram(dialog);
            }
        }

        public void RefreshKeyEntries()
        {
            Identifiers.Clear();
            if (KeyStore != null)
            {
                foreach (var id in KeyStore.GetAll(_keClass))
                {
                    Identifiers.Add(id);
                }
            }
        }

        public void RefreshKeyEntriesView()
        {
            _identifiersView.Refresh();
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
