using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
using log4net;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Leosac.KeyManager.Domain
{
    public class EditKeyStoreControlViewModel : ObservableValidator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType ?? typeof(EditKeyStoreControlViewModel));
        public EditKeyStoreControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _showProgress = false;
            SnackbarMessageQueue = snackbarMessageQueue;
            Tabs = new ObservableCollection<TabItem>();
            RefreshKeyEntriesCommand = new AsyncRelayCommand(() => RefreshKeyEntries(500));
            SaveFavoriteCommand = new RelayCommand(
                () =>
                {
                    DialogHost.CloseDialogCommand.Execute(null, null);
                    var favorites = Favorites.GetSingletonInstance();
                    favorites?.SaveToFile();
                });
        }

        public ISnackbarMessageQueue SnackbarMessageQueue { get; private set; }

        protected readonly IList<KeyEntriesControlViewModel> _keModels = new List<KeyEntriesControlViewModel>();
        protected void OnKeyStoreUpdated()
        {
            if (Favorite != null)
            {
                var favorites = Favorites.GetSingletonInstance();
                if (favorites != null)
                {
                    SaveToFavorite(favorites, Favorite);
                }
            }
        }

        private KeyStore? _keyStore;

        public KeyStore? KeyStore
        {
            get => _keyStore;
            set => SetProperty(ref _keyStore, value);
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

        public ObservableCollection<TabItem> Tabs { get; set; }

        public RelayCommand? HomeCommand { get; set; }

        public AsyncRelayCommand RefreshKeyEntriesCommand { get; }

        public RelayCommand SaveFavoriteCommand { get; }

        public Task CloseKeyStore()
        {
            return CloseKeyStore(true);
        }

        public async Task CloseKeyStore(bool navigate)
        {
            if (KeyStore != null)
            {
                await KeyStore.Close(true);
            }
            KeyStore = null;
            _keModels.Clear();
            Tabs.Clear();
            Favorite = null;

            if (navigate)
            {
                HomeCommand?.Execute(null);
            }
        }

        public async Task OpenKeyStore()
        {
            if (KeyStore == null) return;

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                await KeyStore.Open().ConfigureAwait(true);

                var tempModels = new List<KeyEntriesControlViewModel>();
                var tempTabs = new List<TabItem>();

                foreach (var kclass in KeyStore.SupportedClasses)
                {
                    var model = new KeyEntriesControlViewModel(SnackbarMessageQueue, kclass) { KeyStore = KeyStore };
                    model.DefaultKeyEntryUpdated += (_, _) => OnKeyStoreUpdated();
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
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
            await RefreshKeyEntries().ConfigureAwait(true);
        }

        public Task RefreshKeyEntries() => RefreshKeyEntries(0);

        public async Task RefreshKeyEntries(int delay)
        {
            IsLoadingKeyEntries = true;
            try
            {
                if (delay > 0)
                    await Task.Delay(delay);
                await Task.WhenAll(_keModels.Select(m => m.RefreshKeyEntries()));
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
            if (Favorite != null)
            {
                await EditFavorite(Favorites.GetSingletonInstance(), Favorite);
            }
        }

        public static async Task EditFavorite(Favorites? favorites, Favorite fav)
        {
            var factory = KeyStoreUIFactory.GetFactoryFromPropertyType(fav.Properties?.GetType());
            if (factory != null)
            {
                if (favorites != null)
                {
                    var model = new KeyStoreSelectorDialogViewModel
                    {
                        Message = "Edit the Key Store Favorite"
                    };
                    model.SelectedFactoryItem = model.KeyStoreFactories.Where(item => item.Factory == factory).FirstOrDefault();
                    if (model.SelectedFactoryItem != null)
                    {
                        model.SelectedFactoryItem.DataContext!.Properties = fav.Properties;
                        var dialog = new KeyStoreSelectorDialog
                        {
                            DataContext = model
                        };
                        object? ret = await DialogHost.Show(dialog, "RootDialog");
                        if (ret != null)
                        {
                            fav.Properties = model.SelectedFactoryItem.DataContext.Properties;
                            SaveToFavorite(favorites, fav);
                        }
                    }
                }
            }
        }

        private static void SaveToFavorite(Favorites favorites, Favorite fav)
        {
            int favindex = favorites.KeyStores.IndexOf(fav);
            if (favindex > -1)
            {
                favorites.KeyStores.RemoveAt(favindex);
            }
            favorites.KeyStores.Add(fav);
            favorites.SaveToFile();
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
            var ret = await DialogHost.Show(dialog, "RootDialog");
            return (ret != null);
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
            var ret = await DialogHost.Show(dialog, "RootDialog", closingEventHandler: (_, __) =>
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
                    await DialogHost.Show(batchDialog, "RootDialog");
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
                destStore.KeyEntryRetrieved += (_, _) => Interlocked.Increment(ref _progressValue);
                destStore.KeyEntryUpdated += (_, _) => Interlocked.Increment(ref _progressValue);
                destStore.UserMessageNotified += (_, e) => SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, e);
                if (KeyStore == null)
                    throw new InvalidOperationException("KeyStore can't be null when running operations.");
                KeyStore.Options = model.Options;
                Action<KeyStore, KeyEntryClass, int> initCallback = (_, _, nbEntries) =>
                {
                    ProgressMaximum = nbEntries * 2;
                };
                var favorites = Favorites.GetSingletonInstance();
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

        private async Task ExecuteKeyStoreOperation(Func<Task<bool>> operation, string successMessage, string logErrorMessage)
        {
            try
            {
                if (await operation())
                {
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, successMessage);
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
        }

        public async Task Publish()
        {
            await ExecuteKeyStoreOperation(
                async () => KeyStore != null &&
                    await RunOnKeyStore(() => new PublishKeyStoreDialog(), KeyStore.Publish, "Publish Key Entries"),
                "Key Entries have been successfully published.",
                "Publishing the Key Entries failed unexpectedly."
            );
        }

        public async Task Import()
        {
            await ExecuteKeyStoreOperation(
                async () => KeyStore != null &&
                    await RunOnKeyStore(() => new PublishKeyStoreDialog(), KeyStore.Import, Properties.Resources.ImportKeyStore),
                "Key Entries have been successfully imported.",
                "Importing the Key Entries failed unexpectedly."
            );
        }

        public async Task Diff()
        {
            await ExecuteKeyStoreOperation(
                async () => KeyStore != null &&
                    await RunOnKeyStore(() => new DiffKeyStoreDialog(), KeyStore.Diff, Properties.Resources.DiffKeyStore),
                "No differences found.",
                "Comparing the Key Entries failed unexpectedly."
            );
        }
    }
}
