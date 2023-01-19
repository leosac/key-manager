using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.Wizard.SAMAccessControl.Domain;
using System.Windows;

namespace Leosac.KeyManager.Library.Wizard.SAMAccessControl
{
    public class SAMAccessControlWizardFactory : WizardFactory
    {
        public override string Name => Properties.Resources.FactoryName;

        public override Window CreateWizardWindow()
        {
            return new SAMAccessControlWizardWindow();
        }

        public override IList<KeyStore.KeyEntry> GetKeyEntries(Window window)
        {
            if (window is SAMAccessControlWizardWindow w)
            {
                var model = w.DataContext as SAMAccessControlWizardWindowViewModel;
            }
        }
    }
}