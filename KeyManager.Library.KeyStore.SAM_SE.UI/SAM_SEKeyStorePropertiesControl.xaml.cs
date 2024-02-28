using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI
{
    /// <summary>
    /// Logique d'interaction pour SAM_SEKeyStorePropertiesControl.xaml
    /// </summary>
    public partial class SAM_SEKeyStorePropertiesControl : UserControl
    {
        public SAM_SEKeyStorePropertiesControl()
        {
            InitializeComponent();
        }

        private void RefreshReaderUnitsButton(object sender, RoutedEventArgs e)
        {
            if (DataContext is SAM_SEKeyStorePropertiesControlViewModel model)
            {
                RefreshReaderUnits((Button)sender);
            }
        }
        private void RefreshReaderUnitsComboBox(object sender, RoutedEventArgs e)
        {
            if (DataContext is SAM_SEKeyStorePropertiesControlViewModel model)
            {
                RefreshReaderUnits((ComboBox)sender);
            }
        }

        private async void RefreshReaderUnits(UIElement ele)
        {
            if (DataContext is SAM_SEKeyStorePropertiesControlViewModel model)
            {
                ele.IsEnabled = false;
                try
                {
                    await model.RefreshStationProgrammationList();
                }
                finally
                {
                    ele.IsEnabled = true;
                }
            }
        }
    }
}
