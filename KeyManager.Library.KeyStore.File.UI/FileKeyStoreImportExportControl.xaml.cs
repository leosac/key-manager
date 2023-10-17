using Leosac.KeyManager.Library.KeyStore.File.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.File.UI
{
    /// <summary>
    /// Interaction logic for FileKeyStoreImportExportControl.xaml
    /// </summary>
    public partial class FileKeyStoreImportExportControl : UserControl
    {
        public FileKeyStoreImportExportControl()
        {
            InitializeComponent();

            DataContext = new FileKeyStoreImportExportControlViewModel();
        }
    }
}
