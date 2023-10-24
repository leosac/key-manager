using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Leosac.KeyManager.Domain
{
    public class EditKeyStoreControlViewModel : ObservableValidator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        public EditKeyStoreControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _showProgress = false;
            _snackbarMessageQueue = snackbarMessageQueue;
            Tabs = new ObservableCollection<TabItem>();
            _keModels = new List<KeyEntriesControlViewModel>();
            RefreshKeyEntriesCommand = new AsyncRelayCommand(() => RefreshKeyEntries(500));
            SaveFavoriteCommand = new RelayCommand(
                () =>
                {
                    Flipper.FlipCommand.Execute(null, null);
                    var favorites = Favorites.GetSingletonInstance();
                    favorites?.SaveToFile();
                });
        }

        protected ISnackbarMessageQueue _snackbarMessageQueue;

        protected IList<KeyEntriesControlViewModel> _keModels;

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

        public void CloseKeyStore()
        {
            CloseKeyStore(true);
        }

        public void CloseKeyStore(bool navigate)
        {
            KeyStore?.Close();
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
            if (KeyStore != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    await KeyStore.Open();
                    var classes = KeyStore.SupportedClasses;
                    foreach (var kclass in classes)
                    {
                        var model = new KeyEntriesControlViewModel(_snackbarMessageQueue, kclass) { KeyStore = KeyStore };
                        _keModels.Add(model);
                        Tabs.Add(new TabItem
                        {
                            Header = string.Format("{0} Key Entries", kclass.ToString()),
                            Content = new KeyEntriesControl
                            {
                                DataContext = model
                            }
                        });
                    }
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
                await RefreshKeyEntries();
            }
        }

        public Task RefreshKeyEntries() => RefreshKeyEntries(0);

        public async Task RefreshKeyEntries(int delay)
        {
            IsLoadingKeyEntries = true;
            if (delay > 0)
            {
                await Task.Delay(delay);
            }
            try
            {
                foreach (var model in _keModels)
                {
                    await model.RefreshKeyEntries();
                }
            }
            catch(Exception ex)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
            }
            IsLoadingKeyEntries = false;
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
                    int favindex = favorites.KeyStores.IndexOf(fav);
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
                            if (favindex > -1)
                            {
                                favorites.KeyStores.RemoveAt(favindex);
                            }
                            favorites.KeyStores.Add(fav);
                            favorites.SaveToFile();
                        }
                    }
                }
            }
        }

        public async Task Publish()
        {
            if (KeyStore != null)
            {
                var model = new PublishKeyStoreDialogViewModel();
                var dialog = new PublishKeyStoreDialog
                {
                    DataContext = model
                };
                object? ret = await DialogHost.Show(dialog, "RootDialog");
                if (ret != null && model.Favorite != null)
                {
                    var prop = model.Favorite.Properties;
                    if (prop != null)
                    {
                        var factory = KeyStoreFactory.GetFactoryFromPropertyType(prop.GetType());
                        if (factory != null)
                        {
                            try
                            {
                                ProgressValue = 0;
                                ShowProgress = true;

                                var deststore = factory.CreateKeyStore();
                                deststore.Properties = prop;
                                deststore.KeyEntryRetrieved += (sender, e) => ProgressValue++;
                                deststore.KeyEntryUpdated += (sender, e) => ProgressValue++;
                                var initCallback = new Action<KeyStore, KeyEntryClass, int>((_, _, nbentries) =>
                                {
                                    ProgressMaximum = nbentries * 2;
                                });
                                var getFavoriteKeyStore = new Func<string, KeyStore?>((favoriteName) =>
                                {
                                    if (!string.IsNullOrEmpty(favoriteName))
                                    {
                                        var favorites = Favorites.GetSingletonInstance();
                                        if (favorites != null)
                                        {
                                            var fav = favorites.KeyStores.Where(ks => ks.Name.ToLowerInvariant() == favoriteName.ToLowerInvariant()).SingleOrDefault();
                                            if (fav != null)
                                            {
                                                return fav.CreateKeyStore();
                                            }
                                            else
                                            {
                                                log.Error(string.Format("Cannot found the favorite Key Store `{0}`.", favoriteName));
                                                throw new KeyStoreException("Cannot found the favorite Key Store.");
                                            }
                                        }
                                    }
                                    return null;
                                });

                                foreach(var keModel in _keModels)
                                {
                                    if (keModel.ShowSelection)
                                    {
                                        var entries = keModel.Identifiers.Where(k => k.Selected && k.KeyEntryId != null).Select(k => k.KeyEntryId!);
                                        await KeyStore.Publish(deststore,
                                            getFavoriteKeyStore,
                                            entries,
                                            keModel.KeyEntryClass,
                                            model.WrappingKeyId,
                                            model.WrappingKeySelector,
                                            initCallback
                                        );
                                    }
                                    else
                                    {
                                        await KeyStore.Publish(deststore,
                                            getFavoriteKeyStore,
                                            keModel.KeyEntryClass,
                                            model.WrappingKeyId,
                                            model.WrappingKeySelector,
                                            initCallback
                                        );
                                    }
                                }

                                SnackbarHelper.EnqueueMessage(_snackbarMessageQueue, "Key Entries have been successfully published.");
                            }
                            catch (KeyStoreException ex)
                            {
                                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
                            }
                            catch (Exception ex)
                            {
                                log.Error("Publishing the Key Entries failed unexpected.", ex);
                                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
                            }
                            finally
                            {
                                ShowProgress = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
