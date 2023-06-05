using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyEntriesControl.xaml
    /// </summary>
    public partial class KeyEntriesControl : UserControl
    {
        public KeyEntriesControl()
        {
            InitializeComponent();
        }

        public KeyEntriesControlViewModel? KeyEntriesDataContext => DataContext as KeyEntriesControlViewModel;

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KeyEntriesDataContext?.RefreshKeyEntries();
        }

        private void KeyEntryEdit_OnDialogClosed(object sender, DialogClosedEventArgs e)
        {
            if (e.Parameter is KeyStore.KeyEntry keyEntry)
            {
                KeyEntriesDataContext?.KeyStore?.Update(keyEntry);
            }
        }

        private void KeyEntryDeletion_OnDialogClosed(object sender, DialogClosedEventArgs e)
        {
            if (e.Parameter is KeyEntryId identifier)
            {
                KeyEntriesDataContext?.DeleteKeyEntryCommand?.Execute(identifier);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            KeyEntriesDataContext!.SearchTerms = SearchTerms.Text;
            KeyEntriesDataContext.RefreshKeyEntriesView();
        }

        private void SearchTerms_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                KeyEntriesDataContext!.SearchTerms = SearchTerms.Text;
                KeyEntriesDataContext.RefreshKeyEntriesView();
                e.Handled = true;
            }
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            SnackbarHelper.HandlePreviewMouseWheel(sender, e);
        }
    }
}
