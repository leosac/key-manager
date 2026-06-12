using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.KeyGen;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin.UI;
using Leosac.KeyManager.Library.UI.Helpers;
using Leosac.WpfApp;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyEntriesControlViewModel : ObservableValidator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private const string RootDialog = "RootDialog";

        public KeyEntriesControlViewModel(ISnackbarMessageQueue snackbarMessageQueue, KeyEntryClass keClass)
        {
            _snackbarMessageQueue = snackbarMessageQueue;
            KeyEntryClass = keClass;
            _identifierLock = new object();
            Identifiers = new ObservableCollection<SelectableKeyEntryId>();
            BindingOperations.EnableCollectionSynchronization(Identifiers, _identifierLock);
            var factories = WizardFactory.RegisteredFactories.Where(f => f.KeyEntryClasses.Contains(KeyEntryClass));
            WizardFactories = new ObservableCollection<WizardFactory>(factories);

            CreateKeyEntryCommand = new AsyncRelayCommand(CreateKeyEntryAsync);
            GenerateKeyEntryCommand = new AsyncRelayCommand(GenerateKeyEntryAsync);

            EditDefaultKeyEntryCommand = new AsyncRelayCommand(
                async () =>
                {
                    if (await EditDefaultKeyEntryAsync())
                    {
                        OnDefaultKeyEntryUpdated();
                    }
                });

            EditKeyEntryCommand = new AsyncRelayCommand<SelectableKeyEntryId>(EditKeyEntryAsync);
            CopyKeyEntryCommand = new AsyncRelayCommand<SelectableKeyEntryId>(CopyKeyEntryAsync);
            MoveUpKeyEntryCommand = new AsyncRelayCommand<SelectableKeyEntryId>(MoveUpKeyEntryAsync);
            MoveDownKeyEntryCommand = new AsyncRelayCommand<SelectableKeyEntryId>(MoveDownKeyEntryAsync);
            DeleteKeyEntryCommand = new AsyncRelayCommand<SelectableKeyEntryId>(DeleteKeyEntryAsync);
            ImportCryptogramCommand = new AsyncRelayCommand<SelectableKeyEntryId>(ImportCryptogramAsync);

            WizardCommand = new AsyncRelayCommand<WizardFactory>(RunWizardAsync);

            ShowSelectionChangedCommand = new RelayCommand(OnShowSelectionChanged);

            _identifiersView = CollectionViewSource.GetDefaultView(Identifiers);
            _identifiersView.Filter = KeyEntryIdentifiersFilter;
            Identifiers.CollectionChanged += Identifiers_CollectionChanged;

            ToggleSelectionCommand = new RelayCommand(OnToggleSelection);
            ToggleSelectVisibleCommand = new RelayCommand(OnToggleSelectVisible);

            PrintSelectionCommand = new AsyncRelayCommand(PrintSelection);
            SearchCommand = new RelayCommand(() => RefreshKeyEntriesView());
            OrderingCommand = new RelayCommand<string>(Ordering);

            var uipref = UIPreferences.GetSingletonInstance(false);
            if (!string.IsNullOrEmpty(uipref?.DefaultOrdering))
            {
                Ordering(uipref.DefaultOrdering);
            }
        }

        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly object _identifierLock;
        private KeyStore.KeyStore? _keyStore;
        private bool _showSelection;
        private readonly ICollectionView _identifiersView;
        private string? _searchTerms;

        private int _selectedIdentifiersCount;

        public int SelectedIdentifiersCount
        {
            get => _selectedIdentifiersCount;
            private set => SetProperty(ref _selectedIdentifiersCount, value);
        }

        private int _visibleIdentifiersCount;
        public int VisibleIdentifiersCount
        {
            get => _visibleIdentifiersCount;
            private set => SetProperty(ref _visibleIdentifiersCount, value);
        }

        private void UpdateVisibleIdentifiersCount()
        {
            VisibleIdentifiersCount = _identifiersView.Cast<object>().Count();
        }

        private void UpdateSelectedIdentifiersCount()
        {
            SelectedIdentifiersCount = Identifiers.Count(i => i.Selected);
        }

        private void Identifier_PropertyChanged(object? _, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SelectableKeyEntryId.Selected))
                return;
            UpdateSelectedIdentifiersCount();
        }

        private void Identifiers_CollectionChanged(object? _, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SelectableKeyEntryId item in e.NewItems)
                {
                    item.PropertyChanged += Identifier_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SelectableKeyEntryId item in e.OldItems)
                {
                    item.PropertyChanged -= Identifier_PropertyChanged;
                }
            }

            UpdateVisibleIdentifiersCount();
            UpdateSelectedIdentifiersCount();
        }

        public event EventHandler? DefaultKeyEntryUpdated;

        protected virtual void OnDefaultKeyEntryUpdated()
        {
            DefaultKeyEntryUpdated?.Invoke(this, EventArgs.Empty);
        }

        protected KeyEntryDialogViewModel CreateKeyEntryDialogViewModel(bool keClone = true)
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

        private Task CreateKeyEntryAsync()
        {
            return CreateKeyEntryAsync(CreateKeyEntryDialogViewModel());
        }

        private async Task CreateKeyEntryAsync(KeyEntryDialogViewModel model)
        {
            bool retry;
            do
            {
                retry = false;
                var dialog = new KeyEntryDialog { DataContext = model };
                var result = await DialogHelper.ForceShow(dialog, RootDialog);
                if (result == null)
                    return;
                if (KeyStore == null || model.KeyEntry == null)
                    return;
                try
                {
                    await KeyStore.Create(model.KeyEntry);
                    Identifiers.Add(new SelectableKeyEntryId
                    {
                        Selected = false,
                        KeyEntryId = model.KeyEntry.Identifier
                    });
                    return;
                }
                catch (Exception ex)
                {
                    HandleOperationException(ex, "Creating the Key Entry");
                    retry = true;
                }
            } while (retry);
        }

        public AsyncRelayCommand GenerateKeyEntryCommand { get; }

        private async Task GenerateKeyEntryAsync()
        {
            var model = CreateKeyEntryDialogViewModel();
            model.ShowKeyMaterials = false;
            bool retry;

            do
            {
                retry = false;
                var dialog = new KeyEntryDialog { DataContext = model };
                var result = await DialogHelper.ForceShow(dialog, RootDialog);
                if (result == null)
                    return;
                if (KeyStore == null || model.KeyEntry == null)
                    return;

                try
                {
                    var id = await KeyStore.Generate(model.KeyEntry);
                    Identifiers.Add(new SelectableKeyEntryId
                    {
                        Selected = false,
                        KeyEntryId = id
                    });
                    return;
                }
                catch (Exception ex)
                {
                    HandleOperationException(ex, "Generating the Key Entry");
                    retry = true;
                }
            } while (retry);
        }

        public AsyncRelayCommand EditDefaultKeyEntryCommand { get; }

        private async Task<bool> EditDefaultKeyEntryAsync()
        {
            var model = CreateKeyEntryDialogViewModel(false);
            var dialog = new KeyEntryDialog { DataContext = model };
            var result = await DialogHelper.ForceShow(dialog, RootDialog);
            if (result == null)
                return false;
            if (KeyStore == null || model.KeyEntry == null)
                return false;
            KeyStore.DefaultKeyEntries[KeyEntryClass] = model.KeyEntry;
            return true;
        }

        public AsyncRelayCommand<SelectableKeyEntryId> EditKeyEntryCommand { get; }

        public AsyncRelayCommand<SelectableKeyEntryId> CopyKeyEntryCommand { get; }

        private async Task EditKeyEntryAsync(SelectableKeyEntryId? identifier)
        {
            try
            {
                if (KeyStore == null || identifier?.KeyEntryId == null)
                    return;
                var model = CreateKeyEntryDialogViewModel();
                model.CanChangeFactory = false;
                model.AllowSubmit = KeyStore.CanUpdateKeyEntries;
                model.SubmitButtonText = Properties.Resources.Update;
                model.SetKeyEntry(await KeyStore.Get(identifier.KeyEntryId, KeyEntryClass));
                if (await UpdateKeyEntryAsync(model))
                {
                    identifier.KeyEntryId = model.KeyEntry?.Identifier;
                }
            }
            catch (Exception ex)
            {
                HandleOperationException(ex, "Loading the Key Entry for update");
            }
        }

        private async Task CopyKeyEntryAsync(SelectableKeyEntryId? identifier)
        {
            if (KeyStore == null || identifier?.KeyEntryId == null)
                return;

            try
            {
                var model = CreateKeyEntryDialogViewModel();
                var keyEntry = await KeyStore.Get(identifier.KeyEntryId, KeyEntryClass);
                if (keyEntry == null)
                    return;

                model.SetKeyEntry(keyEntry);
                await CreateKeyEntryAsync(model);
            }
            catch (Exception ex)
            {
                HandleOperationException(ex, "Copying the Key Entry");
            }
        }

        private async Task<bool> UpdateKeyEntryAsync(KeyEntryDialogViewModel model)
        {
            bool retry;
            do
            {
                retry = false;
                var dialog = new KeyEntryDialog { DataContext = model };
                var result = await DialogHelper.ForceShow(dialog, RootDialog);
                if (result == null)
                    return false;
                if (KeyStore == null || model.KeyEntry == null)
                    return false;
                try
                {
                    await KeyStore.Update(model.KeyEntry);
                    return true;
                }
                catch (Exception ex)
                {
                    HandleOperationException(ex, "Updating the Key Entry");
                    retry = true;
                }
            } while (retry);
            return false;
        }

        public AsyncRelayCommand<SelectableKeyEntryId> DeleteKeyEntryCommand { get; }

        private async Task DeleteKeyEntryAsync(SelectableKeyEntryId? identifier)
        {
            if (identifier?.KeyEntryId == null || KeyStore == null)
                return;
            try
            {
                await KeyStore.Delete(identifier.KeyEntryId, KeyEntryClass);
                Identifiers.Remove(identifier);
            }
            catch (Exception ex)
            {
                HandleOperationException(ex, "Deleting the Key Entry");
            }
        }

        public AsyncRelayCommand<SelectableKeyEntryId> MoveUpKeyEntryCommand { get; }

        private async Task MoveUpKeyEntryAsync(SelectableKeyEntryId? identifier)
        {
            if (identifier?.KeyEntryId == null || KeyStore == null)
                return;
            try
            {
                await KeyStore.MoveUp(identifier.KeyEntryId, KeyEntryClass);
                var oldIndex = Identifiers.IndexOf(identifier);
                if (oldIndex > 0)
                {
                    Identifiers.Move(oldIndex, oldIndex - 1);
                }
            }
            catch (Exception ex)
            {
                HandleOperationException(ex, "Moving Up the Key Entry");
            }
        }

        public AsyncRelayCommand<SelectableKeyEntryId> MoveDownKeyEntryCommand { get; }

        private async Task MoveDownKeyEntryAsync(SelectableKeyEntryId? identifier)
        {
            if (identifier?.KeyEntryId == null || KeyStore == null)
                return;
            try
            {
                await KeyStore.MoveDown(identifier.KeyEntryId, KeyEntryClass);
                var oldIndex = Identifiers.IndexOf(identifier);
                if (oldIndex != -1 && oldIndex < Identifiers.Count - 1)
                {
                    Identifiers.Move(oldIndex, oldIndex + 1);
                }
            }
            catch (Exception ex)
            {
                HandleOperationException(ex, "Moving Down the Key Entry");
            }
        }

        public AsyncRelayCommand<SelectableKeyEntryId> ImportCryptogramCommand { get; }

        private async Task ImportCryptogramAsync(SelectableKeyEntryId? keyEntryId)
        {
            var model = new ImportCryptogramDialogViewModel
            {
                CanChangeIdentifier = keyEntryId?.KeyEntryId == null || !keyEntryId.KeyEntryId.IsConfigured()
            };

            if (keyEntryId?.KeyEntryId != null)
            {
                model.Cryptogram.Identifier = keyEntryId.KeyEntryId;
            }

            bool retry;
            do
            {
                retry = false;
                var dialog = new ImportCryptogramDialog
                {
                    DataContext = model
                };

                var result = await DialogHelper.ForceShow(dialog, RootDialog);

                if (result == null)
                    return;
                if (KeyStore == null)
                    return;
                if (string.IsNullOrEmpty(model.Cryptogram.Value))
                    return;
                try
                {
                    await KeyStore.Update(model.Cryptogram);
                    return;
                }
                catch (Exception ex)
                {
                    HandleOperationException(ex, "Importing the Key Entry Cryptogram");
                    retry = true;
                }
            } while (retry);
        }

        public AsyncRelayCommand<WizardFactory> WizardCommand { get; }

        private async Task RunWizardAsync(WizardFactory? factory)
        {
            if (factory == null)
                return;
            var w = factory.CreateWizardWindow();
            if (w.ShowDialog() == true)
            {
                try
                {
                    if (KeyStore == null)
                        return;
                    var entries = factory.GetKeyEntries(w);
                    if (entries == null || entries.Count == 0)
                        return;
                    foreach (var entry in entries)
                        await KeyStore.Update(entry, true);
                    await RefreshKeyEntries();
                    SnackbarHelper.EnqueueMessage(_snackbarMessageQueue, "Wizard completed, key entries updated.");
                }
                catch (Exception ex)
                {
                    HandleOperationException(ex, "Updating the key store with resulting key entries from the wizard");
                }
            }
        }

        private void OnShowSelectionChanged()
        {
            if (!ShowSelection)
                ToggleAllSelection(false);
        }

        private void OnToggleSelection()
        {
            var firstVisible = _identifiersView.Cast<SelectableKeyEntryId>().FirstOrDefault();
            if (firstVisible != null)
                ToggleAllSelection(!firstVisible.Selected);
        }

        private void OnToggleSelectVisible()
        {
            var firstVisible = _identifiersView.Cast<SelectableKeyEntryId>().FirstOrDefault();
            if (firstVisible != null)
                AddVisibleToSelection(!firstVisible.Selected);
        }

        public RelayCommand ShowSelectionChangedCommand { get; }

        public RelayCommand ToggleSelectionCommand { get; }

        public RelayCommand ToggleSelectVisibleCommand { get; }

        public AsyncRelayCommand PrintSelectionCommand { get; }

        private void ToggleAllSelection(bool selected)
        {
            var visibleSet = new HashSet<SelectableKeyEntryId>(_identifiersView.Cast<SelectableKeyEntryId>());
            foreach (var identifier in Identifiers)
            {
                if (visibleSet.Contains(identifier))
                    identifier.Selected = selected;
                else
                    identifier.Selected = false;
            }
        }

        private void AddVisibleToSelection(bool selected)
        {
            foreach (SelectableKeyEntryId identifier in _identifiersView)
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

        private static bool IsNumericType(Type type) => NumericTypes.Contains(type);

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
            if (string.IsNullOrWhiteSpace(order))
                return;

            using (new WaitCursor())
            {
                bool isNumeric = KeyStore?.IsNumericKeyId ?? false;
                ListSortDirection direction =
                    order.EndsWith("Desc", StringComparison.OrdinalIgnoreCase)
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;

                using (_identifiersView.DeferRefresh())
                {
                    _identifiersView.SortDescriptions.Clear();

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
                }

                UIPreferences preferences = UIPreferences.GetSingletonInstance(false) ?? new UIPreferences();

                if (!string.Equals(order, preferences.DefaultOrdering, StringComparison.Ordinal))
                {
                    preferences.DefaultOrdering = order;
                    preferences.SaveToFile();
                }
            }
        }

        public async Task RefreshKeyEntries()
        {
            using (new WaitCursor())
            {
                if (KeyStore == null)
                {
                    lock (_identifierLock)
                    {
                        Identifiers.Clear();
                    }
                    return;
                }

                var ids = await KeyStore.GetAll(KeyEntryClass);
                var keyEntryIds = ids.Select(id => new SelectableKeyEntryId { Selected = false, KeyEntryId = id }).ToList();
                lock (_identifierLock)
                {
                    Identifiers.Clear();
                    foreach (var id in keyEntryIds)
                        Identifiers.Add(id);
                }
            }
        }

        public void RefreshKeyEntriesView()
        {
            using (new WaitCursor())
            {
                _identifiersView.Refresh();
                UpdateVisibleIdentifiersCount();
            }
        }

        private bool KeyEntryIdentifiersFilter(object obj)
        {
            if (string.IsNullOrWhiteSpace(_searchTerms))
            {
                return true;
            }

            KeyEntryId? keyEntry = obj switch
            {
                SelectableKeyEntryId selectable => selectable.KeyEntryId,
                KeyEntryId entry => entry,
                _ => null
            };

            if (keyEntry is null)
            {
                return false;
            }

            return ContainsIgnoreCase(keyEntry.Id, _searchTerms) || ContainsIgnoreCase(keyEntry.Label, _searchTerms);
        }

        private static bool ContainsIgnoreCase(string? source, string value)
        {
            return !string.IsNullOrEmpty(source) && source.Contains(value, StringComparison.OrdinalIgnoreCase);
        }

        private void HandleOperationException(Exception ex, string operation)
        {
            if (ex is KeyStoreException keyStoreEx)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, keyStoreEx, "Key Store Error");
                return;
            }
            log.Error($"{operation} failed unexpectedly.", ex);
            SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
        }
    }
}