using Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain;
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

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI
{
    /// <summary>
    /// Interaction logic for SAMKeyStorePropertiesControl.xaml
    /// </summary>
    public partial class SAMKeyStorePropertiesControl : UserControl
    {
        public SAMKeyStorePropertiesControl()
        {
            InitializeComponent();
        }

        private void cbReaderProvider_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is SAMKeyStorePropertiesControlViewModel model)
            {
                model.RefreshReaderList();
            }
        }

        private void btnRefreshReaderUnits_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SAMKeyStorePropertiesControlViewModel model)
            {
                model.RefreshReaderList();
            }
        }
    }
}
