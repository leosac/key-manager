using Leosac.KeyManager.Domain;
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
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for HomeControl.xaml
    /// </summary>
    public partial class HomeControl : UserControl
    {
        public HomeControl()
        {
            InitializeComponent();
        }

        private async void createKeyStore_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var model = new KeyStoreSelectorDialogViewModel() { Message = "Create a new Key Store" };
            var dialog = new KeyStoreSelectorDialog
            {
                DataContext = model
            };

            if (await DialogHost.Show(dialog, "RootDialog") == (object)true)
            {
                var homeModel = DataContext as HomeViewModel;
                if (homeModel != null)
                {
                    var store = model.CreateKeyStore();
                    if (store != null)
                    {
                        store.CreateIfMissing = true;
                        homeModel.KeyStoreCommand?.Execute(store);
                    }
                }
            }
        }

        private async void openKeyStore_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var model = new KeyStoreSelectorDialogViewModel() { Message = "Open an existing Key Store" };
            var dialog = new KeyStoreSelectorDialog
            {
                DataContext = model
            };
            object ret = await DialogHost.Show(dialog, "RootDialog");
            if (ret != null)
            {
                var homeModel = DataContext as HomeViewModel;
                if (homeModel != null)
                {
                    var store = model.CreateKeyStore();
                    homeModel.KeyStoreCommand?.Execute(store);
                }
            }
        }

        private void favoritesKeyStore_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var homeModel = DataContext as HomeViewModel;
            if (homeModel != null)
            {
                homeModel.FavoritesCommand?.Execute(null);
            }
        }
    }
}
