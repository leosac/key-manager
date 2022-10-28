using Leosac.KeyManager.Domain;
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
            var dialog = new KeyStoreSelectorDialog
            {
                DataContext = new KeyStoreSelectorDialogViewModel() { Message = "Create a new Key Store" }
            };

            await DialogHost.Show(dialog, "RootDialog");
        }

        private async void openKeyStore_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new KeyStoreSelectorDialog
            {
                DataContext = new KeyStoreSelectorDialogViewModel() { Message = "Open an existing Key Store" }
            };

            await DialogHost.Show(dialog, "RootDialog");
        }
    }
}
