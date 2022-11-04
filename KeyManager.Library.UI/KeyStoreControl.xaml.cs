using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
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

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyStoreControl.xaml
    /// </summary>
    public partial class KeyStoreControl : UserControl
    {
        public KeyStoreControl()
        {
            InitializeComponent();
        }

        public KeyStoreControlViewModel? KeyStoreDataContext => DataContext as KeyStoreControlViewModel;

        private async void btnCreateKeyEntry_Click(object sender, RoutedEventArgs e)
        {
            var model = new KeyEntryDialogViewModel();
            var dialog = new KeyEntryDialog()
            {
                DataContext = model
            };
            object? ret = await DialogHost.Show(dialog, "RootDialog");
            if (ret != null && model.KeyEntry != null)
            {
                KeyStoreDataContext?.KeyStore?.Create(model.KeyEntry);
                KeyStoreDataContext?.KeyEntryIdentifiers.Add(model.KeyEntry.Identifier);
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KeyStoreDataContext?.RefreshKeyEntries();
        }

        private void KeyEntryEdit_OnDialogClosed(object sender, DialogClosedEventArgs e)
        {
            if (e.Parameter is KeyStore.KeyEntry keyEntry)
            {
                KeyStoreDataContext?.KeyStore?.Update(keyEntry);
            }
        }

        private void KeyEntryDeletion_OnDialogClosed(object sender, DialogClosedEventArgs e)
        {
            if (e.Parameter is string identifier)
            {
                KeyStoreDataContext?.KeyStore?.Delete(identifier);
                KeyStoreDataContext?.KeyEntryIdentifiers.Remove(identifier);
            }
        }
    }
}
