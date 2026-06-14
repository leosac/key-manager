using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.KeyManager.Library.UI.Helpers;
using Leosac.WpfApp;
using Leosac.WpfApp.Abstractions;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Domain
{
    public class EditKeyStoreControlViewModel : ObservableValidator, IStickyHeaderSupport
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType ?? typeof(EditKeyStoreControlViewModel));

        private Favorites? _cachedFavorites;

        private Favorites? GetFavorites() => _cachedFavorites ??= Favorites.GetSingletonInstance();

        private const string RootDialog = "RootDialog";

        internal EventHandler<string>? RegisteredUserMessageHandler;

        public EditKeyStoreControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _showProgress = false;
            SnackbarMessageQueue = snackbarMessageQueue;
            Header = new KeyEntriesHeaderViewModel();
            RefreshKeyEntriesCommand = new AsyncRelayCommand(() => RefreshKeyEntries(500));
            SaveFavoriteCommand = new RelayCommand(
                () =>
                {
                    DialogHost.CloseDialogCommand.Execute(null, null);
                    var favorites = GetFavorites();
                    favorites?.SaveToFile();
                });
        }

        public ISnackbarMessageQueue SnackbarMessageQueue { get; private set; }

        protected readonly IList<KeyEntriesControlViewModel> _keModels = new List<KeyEntriesControlViewModel>();

        private readonly Dictionary<KeyEntriesControlViewModel, EventHandler> _eventHandlers = new();
        private readonly Dictionary<KeyEntriesControlViewModel, PropertyChangedEventHandler> _keyEntryHandlers = new();

        protected void OnKeyStoreUpdated()
        {
            var favorite = Favorite;
            if (favorite == null)
                return;
            var favorites = GetFavorites();
            if (favorites != null)
                SaveToFavorite(favorites, favorite);
        }

        private KeyStore? _keyStore;

        public KeyStore? KeyStore
        {
            get => _keyStore;
            set
            {
                if (SetProperty(ref _keyStore, value))
                    OnKeyStoreChanged();
            }
        }

        private Favorite? _favorite;

        public Favorite? Favorite
        {
            get => _favorite;
            set => SetProperty(ref _favorite, value);
        }

        private bool _showProgress;
        public bool ShowProgress
        {
            get => _showProgress;
            set => SetProperty(ref _showProgress, value);
        }

        private int _progressValue;
        public int ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        private int _progressMaximum;
        public int ProgressMaximum
        {
            get => _progressMaximum;
            set => SetProperty(ref _progressMaximum, value);
        }

        private bool _isLoadingKeyEntries;
        public bool IsLoadingKeyEntries
        {
            get => _isLoadingKeyEntries;
            set => SetProperty(ref _isLoadingKeyEntries, value);
        }

        private bool _supportsStickyHeader;
        public bool SupportsStickyHeader
        {
            get => _supportsStickyHeader;
            set => SetProperty(ref _supportsStickyHeader, value);
        }

        private bool _isToolbarCollapsed;
        public bool IsStickyHeaderVisible =>
            KeyStore != null && _isToolbarCollapsed;

        public void SetToolbarCollapsed(bool collapsed)
        {
            if (_isToolbarCollapsed == collapsed)
                return;
            _isToolbarCollapsed = collapsed;
            OnPropertyChanged(nameof(IsStickyHeaderVisible));
        }

        private KeyEntriesControlViewModel? _activeKeyEntries;
        public KeyEntriesControlViewModel? ActiveKeyEntries
        {
            get => _activeKeyEntries;
            set
            {
                if (SetProperty(ref _activeKeyEntries, value))
                    Header.Current = value;
            }
        }

        public KeyEntriesHeaderViewModel Header { get; }

        public ObservableCollection<TabItem> Tabs { get; } = new();

        public RelayCommand? HomeCommand { get; set; }

        public AsyncRelayCommand RefreshKeyEntriesCommand { get; }

        public RelayCommand SaveFavoriteCommand { get; }

        private void IncrementProgress()
        {
            ProgressValue = Interlocked.Increment(ref _progressValue);
        }

        public async Task CloseKeyStore(bool navigate = true)
        {
            try
            {
                var keyStore = KeyStore;
                if (keyStore != null)
                    await keyStore.Close(true);
                foreach (var model in _keModels.ToList())
                    DetachModel(model);
                _keModels.Clear();
                Tabs.Clear();
                ActiveKeyEntries = null;
                if (keyStore != null && RegisteredUserMessageHandler != null)
                {
                    keyStore.UserMessageNotified -= RegisteredUserMessageHandler;
                    RegisteredUserMessageHandler = null;
                }
                KeyStore = null;
                Favorite = null;
                if (navigate)
                    HomeCommand?.Execute(null);
            }
            catch (Exception ex)
            {
                log.Error("CloseKeyStore failed", ex);
            }
        }

        public async Task OpenKeyStore()
        {
            var keyStore = KeyStore;
            if (keyStore == null)
                return;
            if (_keModels.Count > 0)
            {
                log.Warn("OpenKeyStore called while models are already loaded.");
                ClearKeyEntryModels();
            }
            using (new WaitCursor())
            {
                await keyStore.Open();
                var tempModels = new List<KeyEntriesControlViewModel>();
                var tempTabs = new List<TabItem>();

                foreach (var kclass in keyStore.SupportedClasses)
                {
                    var model = new KeyEntriesControlViewModel(SnackbarMessageQueue, kclass) { KeyStore = KeyStore };
                    AttachModel(model);
                    tempModels.Add(model);
                    tempTabs.Add(new TabItem
                    {
                        Header = $"{kclass} Key Entries",
                        Content = new KeyEntriesControl { DataContext = model }
                    });
                }
                _keModels.Clear();
                Tabs.Clear();
                foreach (var model in tempModels)
                    _keModels.Add(model);
                foreach (var tab in tempTabs)
                    Tabs.Add(tab);
                ActiveKeyEntries = tempModels.FirstOrDefault();
                await RefreshKeyEntries().ConfigureAwait(true);
            }
        }

        private void ClearKeyEntryModels()
        {
            foreach (var model in _keModels.ToList())
                DetachModel(model);

            _keModels.Clear();
            Tabs.Clear();
            ActiveKeyEntries = null;
        }

        private void AttachModel(KeyEntriesControlViewModel model)
        {
            PropertyChangedEventHandler stickyHandler = KeyEntries_PropertyChanged;
            EventHandler defaultKeyHandler = (_, _) => OnKeyStoreUpdated();

            model.PropertyChanged += stickyHandler;
            model.DefaultKeyEntryUpdated += defaultKeyHandler;

            _keyEntryHandlers[model] = stickyHandler;
            _eventHandlers[model] = defaultKeyHandler;
        }

        private void DetachModel(KeyEntriesControlViewModel model)
        {
            if (_keyEntryHandlers.Remove(model, out var propertyHandler))
                model.PropertyChanged -= propertyHandler;
            if (_eventHandlers.Remove(model, out var eventHandler))
                model.DefaultKeyEntryUpdated -= eventHandler;
            model.KeyStore = null;
        }

        private void KeyEntries_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(KeyEntriesControlViewModel.IsToolbarCollapsed) && sender is KeyEntriesControlViewModel vm)
                SetToolbarCollapsed(vm.IsToolbarCollapsed);
        }

        public Task RefreshKeyEntries() => RefreshKeyEntries(0);

        public async Task RefreshKeyEntries(int delay)
        {
            IsLoadingKeyEntries = true;
            try
            {
                if (delay > 0)
                    await Task.Delay(delay);
                var errors = new List<Exception>();
                var refreshTasks = _keModels.Select(async model =>
                {
                    try
                    {
                        await model.RefreshKeyEntries();
                    }
                    catch (Exception ex)
                    {
                        lock (errors)
                        {
                            errors.Add(ex);
                        }
                        log.Error($"Failed refreshing key entries for {model.KeyEntryClass}.", ex);
                    }
                });
                await Task.WhenAll(refreshTasks);
                if (errors.Count > 0)
                    SnackbarHelper.EnqueueError(SnackbarMessageQueue, new AggregateException(errors), "Some key entry classes failed to refresh");
            }
            catch (Exception ex)
            {
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex, "Key Store Error");
            }
            finally
            {
                IsLoadingKeyEntries = false;
            }
        }

        public async Task EditFavorite()
        {
            if (Favorite == null)
                return;
            await EditFavorite(GetFavorites(), Favorite);
        }

        public static async Task EditFavorite(Favorites? favorites, Favorite fav)
        {
            if (favorites == null || fav?.Properties == null)
                return;
            var factory = KeyStoreUIFactory.GetFactoryFromPropertyType(fav.Properties.GetType());
            if (factory == null)
                return;
            var model = new KeyStoreSelectorDialogViewModel
            {
                Message = "Edit the Key Store Favorite"
            };
            var factoryItem = model.KeyStoreFactories.FirstOrDefault(item => item.Factory == factory);
            if (factoryItem == null || factoryItem.DataContext == null)
                return;
            factoryItem.DataContext.Properties = fav.Properties;
            model.SelectedFactoryItem = factoryItem;
            var dialog = new KeyStoreSelectorDialog
            {
                DataContext = model
            };
            var result = await DialogHost.Show(dialog, RootDialog);
            if (result == null)
                return;
            fav.Properties = factoryItem.DataContext.Properties;
            SaveToFavorite(favorites, fav);
        }

        private static void SaveToFavorite(Favorites favorites, Favorite fav)
        {
            var index = favorites.KeyStores.IndexOf(fav);
            if (index >= 0)
                favorites.KeyStores[index] = fav;
            else
                favorites.KeyStores.Add(fav);
            favorites.SaveToFile();
        }
        
        private void OnKeyStoreChanged()
        {
            OnPropertyChanged(nameof(IsStickyHeaderVisible));
            OnPropertyChanged(nameof(SupportsStickyHeader));
        }

        public static async Task<bool> AskForKeyStoreSecretIfRequired(KeyStore ks, string? favoriteName)
        {
            if (ks.Properties != null && (ks.Properties.StoreSecret || !string.IsNullOrEmpty(ks.Properties.Secret)))
            {
                return true;
            }
            var dialog = new OpenFavoriteControl
            {
                DataContext = ks,
                Title = string.Format("{0} - {1}", Properties.Resources.OpenFavorite, favoriteName ?? ks.Name),
                Command = new RelayCommand(() =>
                {
                    DialogHost.CloseDialogCommand.Execute(ks, null);
                })
            };
            var ret = await DialogHost.Show(dialog, RootDialog);
            return ret != null;
        }

        public async Task<bool> RunOnKeyStore(Func<UserControl> createDialog, Func<KeyStore, Func<string, KeyStore?>, Func<KeyStore, string?, Task<bool>>?, KeyEntryClass, IEnumerable<KeyEntryId>?, Action<KeyStore, KeyEntryClass, int>?, Task> action, string? label = null)
        {
            if (KeyStore == null)
                return false;
            var dialog = createDialog();
            var model = new PublishKeyStoreDialogViewModel();
            if (!string.IsNullOrEmpty(label))
                model.Label = label;
            dialog.DataContext = model;
            var ret = await DialogHost.Show(dialog, RootDialog, closingEventHandler: (_, _) =>
            {
                dialog.DataContext = null;
            });
            if (ret == null || model.Favorite == null)
                return false;
            if (model.BatchOptions.IsSupported && model.BatchOptions.IsEnabled)
            {
                var batch = new PublishBatchDialogViewModel(model.Favorite, model.BatchOptions, KeyStore, _keModels);
                var batchDialog = new PublishBatchDialog
                {
                    DataContext = batch
                };
                try
                {
                    await DialogHost.Show(batchDialog, RootDialog);
                }
                finally
                {
                    batch.Dispose();
                }
                return true;
            }
            var prop = model.Favorite.Properties;
            if (prop == null)
                return false;
            var factory = KeyStoreFactory.GetFactoryFromPropertyType(prop.GetType());
            if (factory == null)
                return false;
            try
            {
                ProgressValue = 0;
                ShowProgress = true;
                var destStore = factory.CreateKeyStore()
                    ?? throw new KeyStoreException("Failed to create destination key store.");
                destStore.Properties = prop;
                destStore.Options = model.Options;
                destStore.KeyEntryRetrieved += (_, _) => IncrementProgress();
                destStore.KeyEntryUpdated += (_, _) => IncrementProgress();
                destStore.UserMessageNotified += (_, e) => SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, e);
                if (KeyStore == null)
                    throw new InvalidOperationException("KeyStore can't be null when running operations.");
                KeyStore.Options = model.Options;
                Action<KeyStore, KeyEntryClass, int> initCallback = (_, _, nbEntries) =>
                {
                    ProgressMaximum = nbEntries * 2;
                };
                var favorites = GetFavorites();
                KeyStore? GetFavoriteKeyStore(string favoriteName)
                {
                    if (string.IsNullOrEmpty(favoriteName))
                        return null;
                    if (favorites != null)
                    {
                        var fav = favorites.Get(favoriteName);
                        if (fav != null)
                            return fav.CreateKeyStore();
                    }
                    log.Error($"Cannot found the favorite Key Store `{favoriteName}`.");
                    throw new KeyStoreException("Cannot found the favorite Key Store.");
                }
                foreach (var keModel in _keModels)
                {
                    IEnumerable<KeyEntryId>? entries = null;
                    if (keModel.ShowSelection)
                        entries = keModel.Identifiers.Where(k => k.Selected && k.KeyEntryId != null).Select(k => k.KeyEntryId!);
                    await action(destStore,
                        GetFavoriteKeyStore,
                        AskForKeyStoreSecretIfRequired,
                        keModel.KeyEntryClass,
                        entries,
                        initCallback
                    );
                }
                return true;
            }
            finally
            {
                ShowProgress = false;
            }
        }

        private async Task<bool> ExecuteKeyStoreOperation(Func<Task<bool>> operation, string successMessage, string logErrorMessage)
        {
            try
            {
                if (await operation())
                {
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, successMessage);
                    return true;
                }
            }
            catch (KeyStoreException ex)
            {
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex, "Key Store Error");
            }
            catch (Exception ex)
            {
                log.Error(logErrorMessage, ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
            return false;
        }

        public async Task<bool> Publish() =>
            await ExecuteKeyStoreOperation(
                async () => KeyStore != null &&
                    await RunOnKeyStore(() => new PublishKeyStoreDialog(), KeyStore.Publish, "Publish Key Entries"),
                "Key Entries have been successfully published.",
                "Publishing the Key Entries failed unexpectedly."
            );

        public async Task<bool> Import() =>
            await ExecuteKeyStoreOperation(
                async () => KeyStore != null &&
                    await RunOnKeyStore(() => new PublishKeyStoreDialog(), KeyStore.Import, Properties.Resources.ImportKeyStore),
                "Key Entries have been successfully imported.",
                "Importing the Key Entries failed unexpectedly."
            );

        public async Task<bool> Diff() =>
            await ExecuteKeyStoreOperation(
                async () => KeyStore != null &&
                await RunOnKeyStore(() => new DiffKeyStoreDialog(), KeyStore.Diff, Properties.Resources.DiffKeyStore),
                "No differences found.",
                "Comparing the Key Entries failed unexpectedly."
            );
    }
}