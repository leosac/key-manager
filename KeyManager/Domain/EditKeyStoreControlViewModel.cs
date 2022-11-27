using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using log4net;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Net.Codecrete.QrCodeGenerator.QrSegment;

namespace Leosac.KeyManager.Domain
{
    public class EditKeyStoreControlViewModel : KeyStoreControlViewModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        public EditKeyStoreControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
            : base(snackbarMessageQueue)
        {
            _showProgress = false;
            Tabs = new ObservableCollection<TabItem>(new[]
            {
                new TabItem() { Header = "Key Entries", Content = new KeyStoreControl() { DataContext = this } }
            });
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

        public KeyManagerCommand? HomeCommand { get; set; }

        public void CloseKeyStore(bool navigate = true)
        {
            KeyStore?.Close();
            KeyStore = null;
            SymmetricIdentifiers.Clear();
            Favorite = null;

            if (navigate)
                HomeCommand?.Execute(null);
        }

        public async void EditFavorite()
        {
            if (Favorite != null)
            {
                var factory = KeyStoreFactory.GetFactoryFromPropertyType(Favorite.Properties?.GetType());
                if (factory != null)
                {
                    var favorites = Favorites.GetSingletonInstance();
                    if (favorites != null)
                    {
                        int favindex = favorites.KeyStores.IndexOf(Favorite);
                        var model = new KeyStoreSelectorDialogViewModel()
                        {
                            Message = "Edit the Key Store Favorite"
                        };
                        model.SelectedFactoryItem = model.KeyStoreFactories.Where(item => item.Factory == factory).FirstOrDefault();
                        model.SelectedFactoryItem!.DataContext!.Properties = Favorite.Properties;
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
                            favorites.KeyStores.Add(Favorite);
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
                                    (store, nbentries) =>
                                    {
                                        ProgressMaximum = nbentries * 2;
                                    });
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
