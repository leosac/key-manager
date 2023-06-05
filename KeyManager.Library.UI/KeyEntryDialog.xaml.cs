using Leosac.WpfApp;
using System.Windows.Controls;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyEntryControl.xaml
    /// </summary>
    public partial class KeyEntryDialog : UserControl
    {
        public KeyEntryDialog()
        {
            InitializeComponent();
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            SnackbarHelper.HandlePreviewMouseWheel(sender, e);
        }
    }
}
