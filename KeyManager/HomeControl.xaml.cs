using Leosac.KeyManager.Domain;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Input;

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

        private async void OpenKeyStore_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!UserRoleContext.IsAdministrator)
                return;

            var model = new KeyStoreSelectorDialogViewModel { Message = Properties.Resources.OpenKeyStore };
            var dialog = new KeyStoreSelectorDialog
            {
                DataContext = model
            };
            object? ret = await DialogHost.Show(dialog, "RootDialog");
            if (ret != null)
            {
                if (DataContext is HomeControlViewModel modelViewModel)
                {
                    var store = model.CreateKeyStore();
                    if (modelViewModel.KeyStoreCommand != null)
                    {
                        await modelViewModel.KeyStoreCommand.ExecuteAsync(store);
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
