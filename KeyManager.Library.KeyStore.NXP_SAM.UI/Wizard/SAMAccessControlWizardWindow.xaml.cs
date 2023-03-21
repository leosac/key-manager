using Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Wizard.Domain;
using System.ComponentModel;
using System.Windows;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Wizard
{
    /// <summary>
    /// Interaction logic for SAMAccessControlWizardWindow.xaml
    /// </summary>
    public partial class SAMAccessControlWizardWindow : Window
    {
        public SAMAccessControlWizardWindow()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new SAMAccessControlWizardWindowViewModel();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
