using Microsoft.Win32;
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

namespace Leosac.KeyManager.KeyStore
{
    /// <summary>
    /// Interaction logic for FileKeyStore.xaml
    /// </summary>
    public partial class FileKeyStore : UserControl
    {
        public FileKeyStore()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new FolderBrowserDialog();
            if (ofd.ShowDialog() == true)
            {
                tbxDirectory.Text = ofd.SelectedFolder;
            }
        }
    }
}
