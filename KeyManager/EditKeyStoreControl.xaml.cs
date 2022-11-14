using Leosac.KeyManager.Domain;
using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Leosac.KeyManager
{
    /// <summary>
    /// Interaction logic for EditKeyStoreControl.xaml
    /// </summary>
    public partial class EditKeyStoreControl : UserControl
    {
        public EditKeyStoreControl()
        {
            InitializeComponent();
        }

        private void btnCloseKeyStore_Click(object sender, RoutedEventArgs e)
        {
            CloseKeyStore();
        }

        private void CloseKeyStore()
        {
            if (DataContext is EditKeyStoreControlViewModel model)
            {
                model.KeyStore?.Close();
                model.KeyStore = null;
                model.KeyEntryIdentifiers.Clear();
                model.Favorite = null;

                model.HomeCommand?.Execute(null);
            }
        }

        private void btnToggleFavorite_Checked(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditKeyStoreControlViewModel model)
            {
                if (model.Favorite == null)
                {
                    var favorites = Favorites.GetSingletonInstance();
                    model.Favorite = favorites?.CreateFromKeyStore(model.KeyStore!);
                }
            }
        }

        private void btnToggleFavorite_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditKeyStoreControlViewModel model)
            {
                if (model.Favorite != null)
                {
                    var favorites = Favorites.GetSingletonInstance();
                    if (favorites!.KeyStores.Contains(model.Favorite))
                    {
                        favorites.KeyStores.Remove(model.Favorite);
                        favorites.SaveToFile();
                    }
                    model.Favorite = null;
                }
            }
        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditKeyStoreControlViewModel cmodel && cmodel.Favorite != null)
            {
                var factory = KeyStoreFactory.GetFactoryFromPropertyType(cmodel.Favorite.Properties?.GetType());
                if (factory != null)
                {
                    var favorites = Favorites.GetSingletonInstance();
                    if (favorites != null)
                    {
                        int favindex = favorites.KeyStores.IndexOf(cmodel.Favorite);
                        var model = new KeyStoreSelectorDialogViewModel()
                        {
                            Message = "Edit the Key Store Favorite"
                        };
                        model.SelectedFactoryItem = model.KeyStoreFactories.Where(item => item.Factory == factory).FirstOrDefault();
                        model.SelectedFactoryItem!.DataContext!.Properties = cmodel.Favorite.Properties;
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
                            favorites.KeyStores.Add(cmodel.Favorite);
                            favorites.SaveToFile();
                        }
                    }
                }
            }
        }
    }
}
