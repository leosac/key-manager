using Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.Domain;
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

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI
{
    /// <summary>
    /// Interaction logic for PKCS11KeyStorePropertiesControl.xaml
    /// </summary>
    public partial class PKCS11KeyStorePropertiesControl : UserControl
    {
        public PKCS11KeyStorePropertiesControl()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PKCS11KeyStorePropertiesControlViewModel model)
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "Dll files (*.dll)|*.dll";
                ofd.FileName = model.PKCS11Properties!.LibraryPath;
                ofd.CheckPathExists = true;
                if (ofd.ShowDialog() == true)
                {
                    model.PKCS11Properties.LibraryPath = ofd.FileName;
                }
            }
        }
    }
}
