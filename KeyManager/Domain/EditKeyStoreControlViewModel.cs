using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Domain
{
    public class EditKeyStoreControlViewModel : KMObject
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        public EditKeyStoreControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _showProgress = false;
            _snackbarMessageQueue = snackbarMessageQueue;
            Tabs = new ObservableCollection<TabItem>();
            SaveFavoriteCommand = new LeosacAppCommand(
                parameter =>
                {
                    var favorites = Favorites.GetSingletonInstance();
                    if (favorites != null)
                    {
                        favorites.SaveToFile();
                    }
                });
        }

        protected ISnackbarMessageQueue _snackbarMessageQueue;

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

        public ObservableCollection<TabItem> Tabs { get; set; }

        public LeosacAppCommand? HomeCommand { get; set; }

        public LeosacAppCommand SaveFavoriteCommand { get; }

        public void CloseKeyStore(bool navigate = true)
        {
            KeyStore?.Close();
            KeyStore = null;
            Tabs.Clear();
            Favorite = null;

            if (navigate)
                HomeCommand?.Execute(null);
        }

        public void OpenKeyStore()
        {
            if (KeyStore != null)
            {
                KeyStore.Open();
                var classes = KeyStore.SupportedClasses;
                foreach (var kclass in classes)
                {
                    var model = new KeyEntriesControlViewModel(_snackbarMessageQueue) { KeyEntryClass = kclass, KeyStore = KeyStore };
                    Tabs.Add(new TabItem()
                    {
                        Header = String.Format("{0} Key Entries", kclass.ToString()),
                        Content = new KeyEntriesControl()
                        {
                            DataContext = model
                        }
                    });
                    model.RefreshKeyEntries();
                }
            }
        }

        public async void EditFavorite()
        {
            if (Favorite != null)
            {
                await EditFavorite(Favorites.GetSingletonInstance(), Favorite);
            }
        }

        public static async Task EditFavorite(Favorites favorites, Favorite fav)
        {
            var factory = KeyStoreUIFactory.GetFactoryFromPropertyType(fav.Properties?.GetType());
            if (factory != null)
            {
                if (favorites != null)
                {
                    int favindex = favorites.KeyStores.IndexOf(fav);
                    var model = new KeyStoreSelectorDialogViewModel()
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

        public async void Publish()
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
                                deststore.KeyEntryRetrieved += (sender, e) => { ProgressValue++; };
                                deststore.KeyEntryUpdated += (sender, e) => { ProgressValue++; };
                                KeyStore.Publish(deststore,
                                    (favoriteName) =>
                                    {
                                        if (!string.IsNullOrEmpty(favoriteName))
                                        {
                                            var favorites = Favorites.GetSingletonInstance();
                                            var fav = favorites.KeyStores.Where(ks => ks.Name.ToLower() == favoriteName.ToLower()).SingleOrDefault();
                                            if (fav != null)
                                            {
                                                return fav.CreateKeyStore();
                                            }
                                            else
                                            {
                                                log.Error(String.Format("Cannot found the favorite Key Store `{0}`.", favoriteName));
                                                throw new KeyStoreException("Cannot found the favorite Key Store.");
                                            }
                                        }
                                        return null;
                                    },
                                    model.WrappingKeyId,
                                    model.WrappingKeySelector,
                                    (store, keClass, nbentries) =>
                                    {
                                        ProgressMaximum = nbentries * 2;
                                    }
                                );

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
