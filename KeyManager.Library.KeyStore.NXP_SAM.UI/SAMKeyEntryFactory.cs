using Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI
{
    public abstract class SAMKeyEntryFactory : KeyEntryFactory
    {
        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new SAMKeyEntryProperties();
        }

        public override UserControl CreateKeyEntryPropertiesControl()
        {
            return new SAMKeyEntryPropertiesControl();
        }

        public override KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel()
        {
            return new SAMKeyEntryPropertiesControlViewModel();
        }
    }
}
