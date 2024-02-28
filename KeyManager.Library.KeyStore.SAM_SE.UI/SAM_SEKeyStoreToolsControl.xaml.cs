using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI
{
    /// <summary>
    /// Interaction logic for SAMKeyStoreToolsControl.xaml
    /// </summary>
    public partial class SAM_SEKeyStoreToolsControl : UserControl
    {
        public SAM_SEKeyStoreToolsControl()
        {
            InitializeComponent();

            DataContext = new SAM_SEKeyStoreToolsControlViewModel();
        }
    }
}
