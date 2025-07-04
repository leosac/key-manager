using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin.UI;
using Leosac.WpfApp;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyEntriesControlViewModel : ObservableValidator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        public KeyEntriesControlViewModel(ISnackbarMessageQueue snackbarMessageQueue, KeyEntryClass keClass)
        {
            _snackbarMessageQueue = snackbarMessageQueue;
            KeyEntryClass = keClass;
            _identifierLock = new object();
            Identifiers = new ObservableCollection<SelectableKeyEntryId>();
            BindingOperations.EnableCollectionSynchronization(Identifiers, _identifierLock);
            WizardFactories = new ObservableCollection<WizardFactory>(WizardFactory.RegisteredFactories.Where(f => f.KeyEntryClasses.Contains(KeyEntryClass)));

            CreateKeyEntryCommand = new AsyncRelayCommand(
                async () =>
            {
                var model = CreateKeyEntryDialogViewModel();
                var dialog = new KeyEntryDialog
                {
                    DataContext = model
                };
                await CreateKeyEntry(dialog);
            });

            GenerateKeyEntryCommand = new AsyncRelayCommand(
                async () =>
                {
                    var model = CreateKeyEntryDialogViewModel();
                    model.ShowKeyMaterials = false;
                    var dialog = new KeyEntryDialog
                    {
                        DataContext = model
                    };
                    await GenerateKeyEntry(dialog);
                });

            EditDefaultKeyEntryCommand = new AsyncRelayCommand(
                async () =>
                {
                    var model = CreateKeyEntryDialogViewModel(false);
                    var dialog = new KeyEntryDialog
                    {
                        DataContext = model
                    };
                    if (await EditDefaultKeyEntry(dialog))
                    {
                        OnDefaultKeyEntryUpdated();
                    }
                });

            EditKeyEntryCommand = new AsyncRelayCommand<SelectableKeyEntryId>(
                async identifier =>
            {
                try
                {
                    if (KeyStore != null && identifier?.KeyEntryId != null)
                    {
                        var model = CreateKeyEntryDialogViewModel();
                        model.CanChangeFactory = false;
                        model.AllowSubmit = KeyStore.CanUpdateKeyEntries;
                        model.SubmitButtonText = Properties.Resources.Update;
                        model.SetKeyEntry(await KeyStore.Get(identifier.KeyEntryId, KeyEntryClass));
                        var dialog = new KeyEntryDialog
                        {
                            DataContext = model
                        };

                        if (await UpdateKeyEntry(dialog) && model.KeyEntry != null)
                        {
                            identifier.KeyEntryId = model.KeyEntry.Identifier;
                        }
                    }
                }
                catch (KeyStoreException ex)
                {
                    SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
                }
                catch (Exception ex)
                {
                    log.Error("Unexpected error occured.", ex);
                }
            });

            CopyKeyEntryCommand = new AsyncRelayCommand<SelectableKeyEntryId>(
                async identifier =>
                {
                    try
                    {
                        if (KeyStore != null && identifier?.KeyEntryId != null)
                        {
                            var model = CreateKeyEntryDialogViewModel();
                            model.SetKeyEntry(await KeyStore.Get(identifier.KeyEntryId, KeyEntryClass));
                            var dialog = new KeyEntryDialog
                            {
                                DataContext = model
                            };

                            await CreateKeyEntry(dialog);
                        }
                    }
                    catch (KeyStoreException ex)
                    {
                        SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Unexpected error occured.", ex);
                    }
                });

            MoveUpKeyEntryCommand = new AsyncRelayCommand<SelectableKeyEntryId>(
                async keyEntryId =>
            {
                if (keyEntryId != null)
                {
                    await MoveUpKeyEntry(keyEntryId);
                }
            });

            MoveDownKeyEntryCommand = new AsyncRelayCommand<SelectableKeyEntryId>(
                async keyEntryId =>
            {
                if (keyEntryId != null)
                {
                    await MoveDownKeyEntry(keyEntryId);
                }
            });

            DeleteKeyEntryCommand = new AsyncRelayCommand<SelectableKeyEntryId>(
                async keyEntryId =>
            {
                if (keyEntryId != null)
                {
                    await DeleteKeyEntry(keyEntryId);
                }
            });

            ImportCryptogramCommand = new AsyncRelayCommand<SelectableKeyEntryId>(
                async keyEntryId =>
            {
                var model = new ImportCryptogramDialogViewModel
                {
                    CanChangeIdentifier = keyEntryId?.KeyEntryId == null || !keyEntryId.KeyEntryId.IsConfigured()
                };
                if (keyEntryId?.KeyEntryId != null)
                {
                    model.Cryptogram.Identifier = keyEntryId.KeyEntryId;
                }
                var dialog = new ImportCryptogramDialog
                {
                    DataContext = model,
                };
                await ImportCryptogram(dialog);
            });

            WizardCommand = new AsyncRelayCommand<WizardFactory>(
                async factory =>
            {
                if (factory != null)
                {
                    await RunWizard(factory);
                }
            });

            ShowSelectionChangedCommand = new RelayCommand(
                () =>
            {
                if (!ShowSelection)
                {
                    ToggleAllSelection(false);
                }
            });

            ToggleSelectionCommand = new RelayCommand(
                () =>
                {
                    if (Identifiers.Count > 0)
                    {
                        ToggleAllSelection(!Identifiers[0].Selected);
                    }
                });

            PrintSelectionCommand = new AsyncRelayCommand(PrintSelection);

            OrderingCommand = new RelayCommand<string>(Ordering);

            _identifiersView = CollectionViewSource.GetDefaultView(Identifiers);
            _identifiersView.Filter = KeyEntryIdentifiersFilter;
        }

        protected ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly object _identifierLock;
        private KeyStore.KeyStore? _keyStore;
        private bool _showSelection;
        private readonly ICollectionView _identifiersView;
        private string? _searchTerms;

        public event EventHandler? DefaultKeyEntryUpdated;
        protected void OnDefaultKeyEntryUpdated()
        {
            DefaultKeyEntryUpdated?.Invoke(this, new EventArgs());
        }

        protected KeyEntryDialogViewModel CreateKeyEntryDialogViewModel()
        {
            return CreateKeyEntryDialogViewModel(true);
        }

        protected KeyEntryDialogViewModel CreateKeyEntryDialogViewModel(bool keClone)
        {
            var model = new KeyEntryDialogViewModel(KeyEntryClass);
            model.SetKeyEntry(KeyStore?.GetDefaultKeyEntry(KeyEntryClass, keClone));
            model.ShowKeyEntryLabel = (KeyStore?.CanDefineKeyEntryLabel).GetValueOrDefault(true);
            return model;
        }

        public ObservableCollection<SelectableKeyEntryId> Identifiers { get; }

        public ObservableCollection<WizardFactory> WizardFactories { get; }

        public KeyStore.KeyStore? KeyStore
        {
            get => _keyStore;
            set => SetProperty(ref _keyStore, value);
        }

        public KeyEntryClass KeyEntryClass { get; }

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

        public bool ShowSelection
        {
            get => _showSelection;
            set => SetProperty(ref _showSelection, value);
        }

        public AsyncRelayCommand CreateKeyEntryCommand { get; }

        private async Task CreateKeyEntry(KeyEntryDialog dialog)
        {
            var ret = await DialogHelper.ForceShow(dialog, "RootDialog");
            if (ret != null && dialog.DataContext is KeyEntryDialogViewModel model)
            {
                if (KeyStore != null && model.KeyEntry != null)
                {
                    try
                    {
                        await KeyStore.Create(model.KeyEntry);
                        Identifiers.Add(new SelectableKeyEntryId {
                            Selected = false,
                            KeyEntryId = model.KeyEntry.Identifier
                        });
                    }
                    catch (KeyStoreException ex)
                    {
                        SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
                        await CreateKeyEntry(dialog);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Creating the Key Entry failed unexpected.", ex);
                        SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
                        await CreateKeyEntry(dialog);
                    }
                }
            }
        }

        public AsyncRelayCommand GenerateKeyEntryCommand { get; }

        private async Task GenerateKeyEntry(KeyEntryDialog dialog)
        {
            var ret = await DialogHelper.ForceShow(dialog, "RootDialog");
            if (ret != null && dialog.DataContext is KeyEntryDialogViewModel model)
            {
                if (KeyStore != null && model.KeyEntry != null)
                {
                    try
                    {
                        var id = await KeyStore.Generate(model.KeyEntry);
                        Identifiers.Add(new SelectableKeyEntryId
                        {
                            Selected = false,
                            KeyEntryId = id
                        });
                    }
                    catch (KeyStoreException ex)
                    {
                        SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
                        await CreateKeyEntry(dialog);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Generating the Key Entry failed unexpected.", ex);
                        SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
                        await CreateKeyEntry(dialog);
                    }
                }
            }
        }

        public AsyncRelayCommand EditDefaultKeyEntryCommand { get; }

        private async Task<bool> EditDefaultKeyEntry(KeyEntryDialog dialog)
        {
            var ret = await DialogHelper.ForceShow(dialog, "RootDialog");
            if (ret != null && dialog.DataContext is KeyEntryDialogViewModel model)
            {
                if (KeyStore != null && model.KeyEntry != null)
                {
                    KeyStore.DefaultKeyEntries[KeyEntryClass] = model.KeyEntry;
                    return true;
                }
            }
            return false;
        }

        public AsyncRelayCommand<SelectableKeyEntryId> EditKeyEntryCommand { get; }

        public AsyncRelayCommand<SelectableKeyEntryId> CopyKeyEntryCommand { get; }

        private async Task<bool> UpdateKeyEntry(KeyEntryDialog dialog)
        {
            try
            {
                object? ret = await DialogHelper.ForceShow(dialog, "RootDialog");
                if (ret != null && dialog.DataContext is KeyEntryDialogViewModel model)
                {
                    if (KeyStore != null && model.KeyEntry != null)
                    {
                        await KeyStore.Update(model.KeyEntry);
                        return true;
                    }
                }
            }
            catch (KeyStoreException ex)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
                return await UpdateKeyEntry(dialog);
            }
            catch (Exception ex)
            {
                log.Error("Updating the Key Entry failed unexpected.", ex);
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
                return await UpdateKeyEntry(dialog);
            }

            return false;
        }

        public AsyncRelayCommand<SelectableKeyEntryId> DeleteKeyEntryCommand { get; }

        private async Task DeleteKeyEntry(SelectableKeyEntryId identifier)
        {
            try
            {
                if (KeyStore != null && identifier.KeyEntryId != null)
                {
                    await KeyStore.Delete(identifier.KeyEntryId, KeyEntryClass);
                }
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

        public AsyncRelayCommand<SelectableKeyEntryId> MoveUpKeyEntryCommand { get; }

        private async Task MoveUpKeyEntry(SelectableKeyEntryId identifier)
        {
            try
            {
                if (KeyStore != null && identifier.KeyEntryId != null)
                {
                    await KeyStore.MoveUp(identifier.KeyEntryId, KeyEntryClass);
                }
                var oldIndex = Identifiers.IndexOf(identifier);
                if (oldIndex > 0)
                {
                    Identifiers.Move(oldIndex, oldIndex - 1);
                }
            }
            catch (KeyStoreException ex)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
            }
            catch (Exception ex)
            {
                log.Error("Moving Up the Key Entry failed unexpected.", ex);
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
            }
        }

        public AsyncRelayCommand<SelectableKeyEntryId> MoveDownKeyEntryCommand { get; }

        private async Task MoveDownKeyEntry(SelectableKeyEntryId identifier)
        {
            try
            {
                if (KeyStore != null && identifier.KeyEntryId != null)
                {
                    await KeyStore.MoveDown(identifier.KeyEntryId, KeyEntryClass);
                }
                var oldIndex = Identifiers.IndexOf(identifier);
                if (oldIndex != -1 && oldIndex < Identifiers.Count - 1)
                {
                    Identifiers.Move(oldIndex, oldIndex + 1);
                }
            }
            catch (KeyStoreException ex)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
            }
            catch (Exception ex)
            {
                log.Error("Moving Up the Key Entry failed unexpected.", ex);
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
            }
        }

        public AsyncRelayCommand<SelectableKeyEntryId> ImportCryptogramCommand { get; }

        private async Task ImportCryptogram(ImportCryptogramDialog dialog)
        {
            try
            {
                object? ret = await DialogHelper.ForceShow(dialog, "RootDialog");
                if (ret != null && dialog.DataContext is ImportCryptogramDialogViewModel model)
                {
                    if (KeyStore != null && !string.IsNullOrEmpty(model.Cryptogram.Value))
                    {
                        await KeyStore.Update(model.Cryptogram);
                    }
                }
            }
            catch (KeyStoreException ex)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
                await ImportCryptogram(dialog);
            }
            catch (Exception ex)
            {
                log.Error("Importing the Key Entry Cryptogram failed unexpected.", ex);
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
                await ImportCryptogram(dialog);
            }
        }

        public AsyncRelayCommand<WizardFactory> WizardCommand { get; }

        private async Task RunWizard(WizardFactory factory)
        {
            var w = factory.CreateWizardWindow();
            if (w.ShowDialog() == true)
            {
                try
                {
                    var entries = factory.GetKeyEntries(w);
                    if (KeyStore != null && entries != null && entries.Count > 0)
                    {
                        foreach (var entry in entries)
                        {
                            await KeyStore.Update(entry, true);
                        }
                        await RefreshKeyEntries();
                        SnackbarHelper.EnqueueMessage(_snackbarMessageQueue, "Wizard completed, key entries updated.");
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Updating the key store with resulting key entries from the wizard failed.", ex);
                    SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
                }
            }
        }

        public RelayCommand ShowSelectionChangedCommand { get; }

        public RelayCommand ToggleSelectionCommand { get; }

        public AsyncRelayCommand PrintSelectionCommand { get; }

        private void ToggleAllSelection(bool selected)
        {
            foreach (var identifier in Identifiers)
            {
                identifier.Selected = selected;
            }
        }

        private async Task PrintSelection()
        {
            var printDialog = new PrintDialog();
            var control = new KeyEntriesPrintControl();
            foreach (var identifier in Identifiers)
            {
                if (identifier.Selected && identifier.KeyEntryId != null && KeyStore != null)
                {
                    var ke = await KeyStore.Get(identifier.KeyEntryId, KeyEntryClass);
                    if (ke != null)
                    {
                        control.KeyEntries.Add(ke);
                    }
                }
            }

            if (control.KeyEntries.Count > 0 && printDialog.ShowDialog() == true)
            {
                try
                {
                    // TODO: print a FlowDocument instead, as PrintVisual needs control to match 1 page
                    printDialog.PrintVisual(control, "Leosac Key Manager - Key Entries Printing");
                }
                catch (Exception ex)
                {
                    log.Error("Key entries printing error.", ex);
                }
            }
        }

        public RelayCommand<string> OrderingCommand { get; }
        private void Ordering(string? order)
        {
            _identifiersView.SortDescriptions.Clear();
            string idProperty = ((KeyStore?.IsNumericKeyId).GetValueOrDefault(false)) ? "KeyEntryId.NumericId" : "KeyEntryId.Id";
            switch (order)
            {
                case "ByIdAsc":
                    // Even if key id is not enforced to be numeric, we try to order in numeric order first
                    if (!(KeyStore?.IsNumericKeyId).GetValueOrDefault(false))
                    {
                        _identifiersView.SortDescriptions.Add(new SortDescription("KeyEntryId.NumericId", ListSortDirection.Ascending));
                    }
                    _identifiersView.SortDescriptions.Add(new SortDescription(idProperty, ListSortDirection.Ascending));
                    break;
                case "ByIdDesc":
                    if (!(KeyStore?.IsNumericKeyId).GetValueOrDefault(false))
                    {
                        _identifiersView.SortDescriptions.Add(new SortDescription("KeyEntryId.NumericId", ListSortDirection.Descending));
                    }
                    _identifiersView.SortDescriptions.Add(new SortDescription(idProperty, ListSortDirection.Descending));
                    break;
                case "ByLabelAsc":
                    _identifiersView.SortDescriptions.Add(new SortDescription("KeyEntryId.Label", ListSortDirection.Ascending));
                    break;
                case "ByLabelDesc":
                    _identifiersView.SortDescriptions.Add(new SortDescription("KeyEntryId.Label", ListSortDirection.Descending));
                    break;
            }
        }

        public async Task RefreshKeyEntries()
        {
            lock (_identifierLock)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Identifiers.Clear();
            }

            try
            {
                if (KeyStore != null)
                {
                    var ids = await KeyStore.GetAll(KeyEntryClass);
                    foreach (var id in ids)
                    {
                        lock (_identifierLock)
                        {
                            Identifiers.Add(new SelectableKeyEntryId
                            {
                                Selected = false,
                                KeyEntryId = id
                            });
                        }
                    }
                }
            }
            finally
            {
                lock (_identifierLock)
                {
                    Mouse.OverrideCursor = null;
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

            if (obj is SelectableKeyEntryId s && s.KeyEntryId != null)
            {
                obj = s.KeyEntryId;
            }

            if (obj is KeyEntryId item)
            {
                var terms = _searchTerms.ToLowerInvariant();
                if (item.Id != null && item.Id.ToLowerInvariant().Contains(terms))
                {
                    return true;
                }

                if (item.Label != null && item.Label.ToLowerInvariant().Contains(terms))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
