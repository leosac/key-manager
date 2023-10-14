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

        private async void CreateKeyStore_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var model = new KeyStoreSelectorDialogViewModel { Message = Properties.Resources.CreateKeyStore };
            var dialog = new KeyStoreSelectorDialog
            {
                DataContext = model
            };

            object? ret = await DialogHost.Show(dialog, "RootDialog");
            if (ret != null)
            {
                if (DataContext is HomeControlViewModel homeModel)
                {
                    var store = model.CreateKeyStore();
                    if (store != null)
                    {
                        store.CreateIfMissing = true;
                        if (homeModel.KeyStoreCommand != null)
                        {
                            await homeModel.KeyStoreCommand.ExecuteAsync(store);
                        }
                    }
                }
            }
        }

        private async void OpenKeyStore_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var model = new KeyStoreSelectorDialogViewModel { Message = Properties.Resources.OpenKeyStore };
            var dialog = new KeyStoreSelectorDialog
            {
                DataContext = model
            };
            object? ret = await DialogHost.Show(dialog, "RootDialog");
            if (ret != null)
            {
                if (DataContext is HomeControlViewModel homeModel)
                {
                    var store = model.CreateKeyStore();
                    if (homeModel.KeyStoreCommand != null)
                    {
                        await homeModel.KeyStoreCommand.ExecuteAsync(store);
                    }
                }
            }
        }

        private void FavoritesKeyStore_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is HomeControlViewModel homeModel)
            {
                homeModel.FavoritesCommand?.Execute(null);
            }
        }
    }
}
