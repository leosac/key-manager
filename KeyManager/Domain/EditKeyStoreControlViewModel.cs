using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Net.Codecrete.QrCodeGenerator.QrSegment;

namespace Leosac.KeyManager.Domain
{
    public class EditKeyStoreControlViewModel : KeyStoreControlViewModel
    {
        public EditKeyStoreControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
            : base(snackbarMessageQueue)
        {
            _showProgress = false;
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

        public KeyManagerCommand? HomeCommand { get; set; }

        public void CloseKeyStore()
        {
            KeyStore?.Close();
            KeyStore = null;
            KeyEntryIdentifiers.Clear();
            Favorite = null;

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
                                var deststore = factory.CreateKeyStore();
                                deststore.Properties = prop;

                                var keyentries = new List<KeyEntry>();
                                var ids = KeyStore.GetAll();
                                ProgressValue = 0;
                                ProgressMaximum = ids.Count * 2;
                                ShowProgress = true;
                                foreach (var id in ids)
                                {
                                    var entry = KeyStore.Get(id);
                                    if (entry != null)
                                    {
                                        keyentries.Add(entry);
                                    }
                                    ProgressValue++;
                                }

                                deststore.Open();
                                deststore.KeyEntryUpdated += Store_KeyEntryUpdated;
                                deststore.Store(keyentries);
                                deststore.Close();
                            }
                            catch (KeyStoreException ex)
                            {
                                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex, "Key Store Error");
                            }
                            catch (Exception ex)
                            {
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

        private void Store_KeyEntryUpdated(object? sender, KeyEntry e)
        {
            ProgressValue++;
        }
    }
}
