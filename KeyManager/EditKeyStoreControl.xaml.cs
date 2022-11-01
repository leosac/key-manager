using Leosac.KeyManager.Domain;
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
            var model = DataContext as EditKeyStoreControlViewModel;
            if (model != null)
            {
                model.KeyStore?.Close();
                model.KeyStore = null;
            }
        }
    }
}
