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
    /// Interaction logic for SAMKeyStoreKeyCountControl.xaml
    /// </summary>
    public partial class SAMKeyStoreKeyCounterControl : UserControl
    {
        public SAMKeyStoreKeyCounterControl()
        {
            InitializeComponent();

            DataContext = new SAMKeyStoreKeyCounterControlViewModel();
        }
    }
}
