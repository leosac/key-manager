using Leosac.KeyManager.Library.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    public partial class KeyEntrySearchDialog : UserControl
    {
        public KeyEntrySearchDialog()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            if (DataContext is KeyEntrySearchDialogViewModel viewModel)
            {
                await viewModel.LoadAsync();
            }
        }
    }
}
