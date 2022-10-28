using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
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

namespace Leosac.KeyManager.Library.KeyStore.File.UI
{
    /// <summary>
    /// Interaction logic for FileKeyStore.xaml
    /// </summary>
    public partial class FileKeyStorePropertiesControl : UserControl
    {
        public FileKeyStorePropertiesControl()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var fbdm = new FolderBrowserDialogViewModel();
            var fbd = new FolderBrowserDialog();
            fbd.DataContext = fbdm;
            if (fbd.ShowDialog() == true)
            {
                tbxDirectory.Text = fbdm.SelectedFolder;
            }
        }
    }
}
