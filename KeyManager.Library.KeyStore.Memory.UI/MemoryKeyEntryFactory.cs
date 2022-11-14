using Leosac.KeyManager.Library.KeyStore.Memory.UI.Domain;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.Memory.UI
{
    public class MemoryKeyEntryFactory : KeyEntryFactory
    {
        public override string Name => "Memory Key Entry";

        public override KeyEntry CreateKeyEntry()
        {
            return new MemorySymmetricKeyEntry();
        }

        public override Type GetKeyEntryPropertiesType()
        {
            return typeof(MemoryKeyEntryProperties);
        }

        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new MemoryKeyEntryProperties();
        }

        public override UserControl CreateKeyEntryPropertiesControl()
        {
            return new MemoryKeyEntryPropertiesControl();
        }

        public override KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel()
        {
            return new MemoryKeyEntryPropertiesControlViewModel();
        }
    }
}
