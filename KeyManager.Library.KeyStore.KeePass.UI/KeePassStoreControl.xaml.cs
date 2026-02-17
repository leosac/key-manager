using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KeyManager.Library.KeyStore.KeePass.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class KeePassKeyStoreControl : UserControl
    {
        public KeePassKeyStoreControl()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            // file browser for .kdbx files
        }

        private void BtnRecent_Click(object sender, RoutedEventArgs e)
        {
            // recent db (optional, not sure to keep)
        }

        private void BtnKeyFile_Click(object sender, RoutedEventArgs e)
        {
            // key file browser dialog
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            // connection test
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TxtFilePath.Clear();
            Password.Clear();
            TxtStatus.Text = "Ready to connect • No database selected";
        }
    }
}