using Leosac.KeyManager.Domain;
using System.Windows;
using System.Windows.Controls;

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

        private void userControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is FavoritesControlViewModel model)
            {
                model.RefreshMasterKeyState();
                model.RefreshFavorites();
            }
        }
    }
}
