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

        private void userControl_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as FavoritesControlViewModel)?.RefreshFavorites();
        }
    }
}
