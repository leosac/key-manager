using Leosac.KeyManager.Domain;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.KeyManager.Library.UI;
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
using Leosac.KeyManager.Library;

namespace Leosac.KeyManager
{
    /// <summary>
    /// Interaction logic for FavoritesControl.xaml
    /// </summary>
    public partial class FavoritesControl : UserControl
    {
        public FavoritesControl()
        {
            InitializeComponent();
        }

        private async void btnNewFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FavoritesControlViewModel cmodel)
            {
                var model = new KeyStoreSelectorDialogViewModel() { Message = "Save a new Favorite Key Store" };
                var dialog = new KeyStoreSelectorDialog
                {
                    DataContext = model
                };

                object? ret = await DialogHost.Show(dialog, "RootDialog");
                if (ret != null)
                {
                    var store = model.CreateKeyStore();
                    if (store != null)
                    {
                        cmodel.Favorites?.CreateFromKeyStore(store);
                    }
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FavoritesControlViewModel cmodel)
            {
                cmodel.Favorites = Favorites.LoadFromFile();
            }
        }

        private void FavoriteDeletion_OnDialogClosed(object sender, DialogClosedEventArgs e)
        {
            if (DataContext is FavoritesControlViewModel cmodel && e.Parameter is Favorite favorite)
            {
                cmodel.Favorites?.KeyStores.Remove(favorite);
                cmodel.Favorites?.SaveToFile();
            }
        }

        private void btnOpenFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FavoritesControlViewModel cmodel)
            {
                // We should improve the command handling to avoid this workaround...
                cmodel.KeyStoreCommand?.Execute((sender as Button)?.CommandParameter);
            }
        }
    }
}
