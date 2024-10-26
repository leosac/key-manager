using Leosac.KeyManager.Library.KeyStore.File.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.File.UI
{
    /// <summary>
    /// Interaction logic for FileKeyStoreImportExportControl.xaml
    /// </summary>
    public partial class FileKeyStoreToolsControl : UserControl
    {
        public FileKeyStoreToolsControl()
        {
            DataContext = new FileKeyStoreToolsControlViewModel();

            InitializeComponent();
        }
    }
}
