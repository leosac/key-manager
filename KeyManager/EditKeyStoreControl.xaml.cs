using Leosac.KeyManager.Domain;
using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
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

        private void BtnCloseKeyStore_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditKeyStoreControlViewModel model)
            {
                model.CloseKeyStore();
            }
        }

        private void BtnToggleFavorite_Checked(object sender, RoutedEventArgs e)
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

        private void BtnToggleFavorite_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditKeyStoreControlViewModel model)
            {
                if (model.Favorite != null)
                {
                    var favorites = Favorites.GetSingletonInstance();
                    if (favorites != null && favorites.KeyStores.Contains(model.Favorite))
                    {
                        favorites.KeyStores.Remove(model.Favorite);
                        favorites.SaveToFile();
                    }
                    model.Favorite = null;
                }
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditKeyStoreControlViewModel model)
            {
                await model.EditFavorite();
            }
        }

        private async void BtnPublish_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditKeyStoreControlViewModel model)
            {
                var plan = MaintenancePlan.GetSingletonInstance();
                if (!string.IsNullOrEmpty(plan.LicenseKey))
                {
                    await model.Publish();
                }
                else
                {
                    MaintenancePlan.OpenRegistration();
                }
            }
        }

        private async void BtnDiff_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditKeyStoreControlViewModel model)
            {
                var plan = MaintenancePlan.GetSingletonInstance();
                if (!string.IsNullOrEmpty(plan.LicenseKey))
                {
                    await model.Diff();
                }
                else
                {
                    MaintenancePlan.OpenRegistration();
                }
            }
        }
    }
}
