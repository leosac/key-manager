using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Linq;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for PublishKeyStoreDialog.xaml
    /// </summary>
    public partial class PublishKeyStoreDialog : UserControl
    {
        private bool _synchronizingKeyEntryClasses;

        public PublishKeyStoreDialog()
        {
            InitializeComponent();
        }

        private void KeyEntryClassesLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SynchronizeSelectedKeyEntryClasses((ListBox)sender);
        }

        private void KeyEntryClassesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_synchronizingKeyEntryClasses)
                return;

            if (DataContext is PublishKeyStoreDialogViewModel viewModel)
            {
                viewModel.Options.GenerateForKeyEntryClasses.Clear();
                foreach (var keyEntryClass in ((ListBox)sender).SelectedItems.OfType<KeyEntryClass>())
                {
                    viewModel.Options.GenerateForKeyEntryClasses.Add(keyEntryClass);
                }
            }
        }

        private void SynchronizeSelectedKeyEntryClasses(ListBox listBox)
        {
            if (DataContext is PublishKeyStoreDialogViewModel viewModel)
            {
                _synchronizingKeyEntryClasses = true;
                try
                {
                    foreach (var keyEntryClass in viewModel.Options.GenerateForKeyEntryClasses.ToList())
                    {
                        listBox.SelectedItems.Add(keyEntryClass);
                    }
                }
                finally
                {
                    _synchronizingKeyEntryClasses = false;
                }
            }
        }
    }
}
