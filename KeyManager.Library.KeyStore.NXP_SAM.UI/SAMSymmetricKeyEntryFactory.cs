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
    public class SAMSymmetricKeyEntryFactory : KeyEntryFactory
    {
        public override string Name => "NXP SAM Key Entry";

        public override KeyEntry CreateKeyEntry()
        {
            return new SAMSymmetricKeyEntry();
        }

        public override Type GetKeyEntryPropertiesType()
        {
            return typeof(SAMSymmetricKeyEntryProperties);
        }

        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new SAMSymmetricKeyEntryProperties();
        }

        public override UserControl CreateKeyEntryPropertiesControl()
        {
            return new SAMSymmetricKeyEntryPropertiesControl();
        }

        public override KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel()
        {
            return new SAMSymmetricKeyEntryPropertiesControlViewModel();
        }
    }
}
