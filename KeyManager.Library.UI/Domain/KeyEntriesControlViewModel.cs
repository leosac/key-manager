using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.KeyGen;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin.UI;
using Leosac.WpfApp;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

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
            var factories = WizardFactory.RegisteredFactories.Where(f => f.KeyEntryClasses.Contains(KeyEntryClass));
            WizardFactories = new ObservableCollection<WizardFactory>(factories);

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

            SearchCommand = new RelayCommand(() => RefreshKeyEntriesView());

            OrderingCommand = new RelayCommand<string>(Ordering);

            _identifiersView = CollectionViewSource.GetDefaultView(Identifiers);
            _identifiersView.Filter = KeyEntryIdentifiersFilter;

            var uipref = UIPreferences.GetSingletonInstance(false);
            if (!string.IsNullOrEmpty(uipref?.DefaultOrdering))
            {
                Ordering(uipref.DefaultOrdering);
            }
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
            if (_keyStore != null)
            {
                LinkDialogViewModel.KeyStoreAttributes = _keyStore.Attributes;
            }
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
            set => SetProperty(ref _searchTerms, value);
        }

        public bool ShowSelection
        {
            get => _showSelection;
            set => SetProperty(ref _showSelection, value);
        }

        public RelayCommand SearchCommand { get; }

        private bool _isToolbarCollapsed;

        public bool IsToolbarCollapsed
        {
            get => _isToolbarCollapsed;
            set => SetProperty(ref _isToolbarCollapsed, value);
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
                        Identifiers.Add(new SelectableKeyEntryId
                        {
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

        private static readonly Regex _step1 = new(@"(\P{Ll})(\P{Ll}\p{Ll})", RegexOptions.Compiled);

        private static readonly Regex _step2 = new(@"(\p{Ll})(\P{Ll})", RegexOptions.Compiled);

        private static string FormatPropertyNameForDisplay(string propertyName)
        {
            return _step2.Replace(_step1.Replace(propertyName, "$1 $2"), "$1 $2");
        }

        private async Task PrintSelection()
        {
            var printDialog = new PrintDialog();

            var flow = new FlowDocument();
            var header = new Section() { Background = Brushes.Orange };
            header.Blocks.Add(new Paragraph(new Run(Properties.Resources.KeyEntriesExportPrint) { FontSize = 20, FontWeight = FontWeights.Bold }));
            flow.Blocks.Add(header);
            var content = new Section();
            var table = new Table();
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            var headerGroup = new TableRowGroup();
            var rowsGroup = new TableRowGroup();
            headerGroup.Rows.Add(new TableRow
            {
                Background = Brushes.LightGray,
                FontWeight = FontWeights.Bold,
                Cells =
                {
                    new TableCell(new Paragraph(new Run(Properties.Resources.KeyEntryIdentifier))),
                    new TableCell(new Paragraph(new Run(Properties.Resources.KeyEntryLabel))),
                    new TableCell(new Paragraph(new Run(Properties.Resources.KeyEntryVariant))),
                    new TableCell(new Paragraph(new Run(Properties.Resources.KeyValue))),
                    new TableCell(new Paragraph(new Run(Properties.Resources.KeyEntryProperties)))
                }
            });
            table.RowGroups.Add(headerGroup);
            table.RowGroups.Add(rowsGroup);
            foreach (var identifier in GetSelectedIdentifiers() ?? Identifiers)
            {
                if (identifier.KeyEntryId is null || KeyStore is null)
                    continue;

                var ke = await KeyStore.Get(identifier.KeyEntryId, KeyEntryClass);
                if (ke is null)
                    continue;

                var border = new Thickness(0, 1, 0, 0);
                var vcell = new TableCell() { BorderBrush = Brushes.Black, BorderThickness = border };
                var pcell = new TableCell() { BorderBrush = Brushes.Black, BorderThickness = border };
                rowsGroup.Rows.Add(new TableRow
                {
                    Cells =
                    {
                        new TableCell(new Paragraph(new Run(ke.Identifier.Id))) { BorderBrush = Brushes.Black, BorderThickness = border },
                        new TableCell(new Paragraph(new Run(ke.Identifier.Label))) { BorderBrush = Brushes.Black, BorderThickness = border },
                        new TableCell(new Paragraph(new Run(ke.Variant?.Name))) { BorderBrush = Brushes.Black, BorderThickness = border },
                        vcell,
                        pcell
                    }
                });
                if (ke.Properties != null)
                {
                    var list = new List { FontSize = 12, Margin = new Thickness(0) };
                    var props = ke.Properties.GetType().GetProperties();
                    foreach (var p in props)
                    {
                        if (!p.CanRead || !p.CanWrite)
                            continue;

                        var value = p.GetValue(ke.Properties);

                        if (value == null)
                            continue;

                        var hasBrowsableFalse = p.GetCustomAttributes(true).OfType<BrowsableAttribute>().Any(ba => !ba.Browsable);

                        if (hasBrowsableFalse)
                            continue;

                        bool include = (p.PropertyType == typeof(string) || p.PropertyType.IsEnum)
                            ? !string.IsNullOrWhiteSpace(value.ToString()) : p.PropertyType == typeof(bool)
                            ? (bool)value : IsNumericType(p.PropertyType);
                        if (include)
                            list.ListItems.Add(new ListItem(new Paragraph(new Run(FormatPropertyNameForDisplay(p.Name) + ": " + value))));
                    }
                    pcell.Blocks.Add(list);
                }
                if (ke.Variant != null)
                {
                    var p = new Paragraph() { FontSize = 12 };
                    foreach (var kc in ke.Variant.KeyContainers)
                    {
                        if (p.Inlines.Count > 0)
                            p.Inlines.Add(new LineBreak());
                        if (kc is KeyVersion kv)
                            p.Inlines.Add(new Run(kv.Version + ": "));
                        string c = "-";
                        if (ke.KClass == KeyEntryClass.Symmetric)
                        {
                            var v = kc.Key.GetAggregatedValueAsString();
                            if (!string.IsNullOrEmpty(v))
                            {
                                var kcv = new KCV();
                                var computed = kcv.ComputeKCV(kc.Key);
                                c = string.IsNullOrEmpty(computed) ? "-" : computed;
                            }
                        }
                        p.Inlines.Add(new Run(c));
                    }
                    vcell.Blocks.Add(p);
                }
            }
            flow.Blocks.Add(table);
            var sign = new Section();
            var stable = new Table()
            {
                Columns =
                {
                    new TableColumn(),
                    new TableColumn() { Width = new GridLength(300) }
                }
            };
            stable.RowGroups.Add(new TableRowGroup()
            {
                Rows =
                {
                    new TableRow()
                    {
                        Cells =
                        {
                            new TableCell(new Paragraph()
                            {
                                Inlines =
                                {
                                    new Run(Properties.Resources.Notes + ":") { FontWeight = FontWeights.Bold },
                                    new LineBreak(),
                                    new LineBreak(),
                                    new LineBreak(),
                                    new LineBreak(),
                                    new LineBreak()
                                }
                            })
                            {
                                BorderBrush = Brushes.Black,
                                BorderThickness = new Thickness(1)
                            },
                            new TableCell(new Section()
                            {
                                Blocks =
                                {
                                    new Paragraph(new Run(Properties.Resources.Date + ": " + DateTime.Now.ToShortDateString()))
                                },
                                Margin = new Thickness(5)
                            })
                        }
                    }
                }
            });
            sign.Blocks.Add(stable);
            flow.Blocks.Add(sign);

            var footer = new Section() { Background = Brushes.Orange };
            footer.Blocks.Add(new Paragraph()
            {
                Inlines =
                {
                    new Run(Properties.Resources.KeyExportDisclaimer1),
                    new LineBreak(),
                    new Run(Properties.Resources.KeyExportDisclaimer2)
                },
                TextAlignment = TextAlignment.Center,
                FontSize = 12
            });
            var fp = new Paragraph(new Run(Properties.Resources.DocumentGenerated)) { Margin = new Thickness(5) };
            fp.Inlines.Add(new Run(" Leosac Key Manager ") { FontWeight = FontWeights.Bold });
            fp.Inlines.Add(new Run(" - "));
            fp.Inlines.Add(new Hyperlink(new Run("www.leosac.com")) { NavigateUri = new Uri("https://www.leosac.com") });
            footer.Blocks.Add(fp);
            flow.Blocks.Add(footer);

            printDialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
            if (printDialog.ShowDialog() == true)
            {
                try
                {
                    flow.PageHeight = printDialog.PrintableAreaHeight;
                    flow.PageWidth = printDialog.PrintableAreaWidth;
                    flow.ColumnGap = 0;
                    flow.ColumnWidth = double.MaxValue;

                    var idocument = flow as IDocumentPaginatorSource;
                    printDialog.PrintDocument(idocument.DocumentPaginator, "Leosac Key Manager - Key Entries Printing");
                }
                catch (Exception ex)
                {
                    log.Error("Key entries printing error.", ex);
                }
            }
        }

        private static readonly HashSet<Type> NumericTypes =
            [typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal)];

        private static bool IsNumericType(Type t)
        {
            return NumericTypes.Contains(t);
        }

        private IEnumerable<SelectableKeyEntryId>? GetSelectedIdentifiers()
        {
            if (!ShowSelection)
                return null;

            var identifiers = new List<SelectableKeyEntryId>();
            foreach (var obj in _identifiersView)
            {
                if (obj is SelectableKeyEntryId identifier)
                {
                    if (identifier.Selected && identifier.KeyEntryId != null)
                    {
                        identifiers.Add(identifier);
                    }
                }
            }
            return identifiers;
        }

        public RelayCommand<string> OrderingCommand { get; }
        private void Ordering(string? order)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                _identifiersView.SortDescriptions.Clear();

                if (string.IsNullOrWhiteSpace(order))
                    return;

                bool isNumeric = (KeyStore?.IsNumericKeyId).GetValueOrDefault(false);
                ListSortDirection direction = order.EndsWith("Desc",
                    StringComparison.OrdinalIgnoreCase) ? ListSortDirection.Descending : ListSortDirection.Ascending;

                void AddSort(string propertyName, ListSortDirection sortDirection)
                {
                    _identifiersView.SortDescriptions.Add(new SortDescription(propertyName, sortDirection));
                }

                switch (order)
                {
                    case "ByIdAsc":
                    case "ByIdDesc":
                        AddSort("KeyEntryId.NumericId", direction);
                        if (!isNumeric)
                            AddSort("KeyEntryId.Id", direction);
                        AddSort("KeyEntryId.Label", ListSortDirection.Ascending);
                        break;
                    case "ByLabelAsc":
                    case "ByLabelDesc":
                        AddSort("KeyEntryId.Label", direction);
                        AddSort("KeyEntryId.NumericId", direction);
                        if (!isNumeric)
                            AddSort("KeyEntryId.Id", direction);
                        break;
                    default:
                        log.Warn($"Unknown ordering mode '{order}'.");
                        return;
                }

                UIPreferences preferences = UIPreferences.GetSingletonInstance(false) ?? new UIPreferences();

                if (!string.Equals(order, preferences.DefaultOrdering, StringComparison.Ordinal))
                {
                    preferences.DefaultOrdering = order;
                    preferences.SaveToFile();
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
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
            Mouse.OverrideCursor = Cursors.Wait;
            _identifiersView.Refresh();
            Mouse.OverrideCursor = null;
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