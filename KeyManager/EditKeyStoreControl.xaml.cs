using Leosac.KeyManager.Domain;
using Leosac.KeyManager.Library.UI;
using Leosac.SharedServices;
using Leosac.WpfApp;
using System.Windows;
using System.Windows.Controls;

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
                    MaintenancePlanHelper.OpenRegistration();
                }
            }
        }

        private async void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditKeyStoreControlViewModel model)
            {
                var plan = MaintenancePlan.GetSingletonInstance();
                if (!string.IsNullOrEmpty(plan.LicenseKey))
                {
                    await model.Import();
                    await model.RefreshKeyEntries();
                }
                else
                {
                    MaintenancePlanHelper.OpenRegistration();
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
                    MaintenancePlanHelper.OpenRegistration();
                }
            }
        }
    }
}
