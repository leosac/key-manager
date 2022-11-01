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
    public class MemoryKeyStoreFactory : KeyStoreFactory
    {
        public override string Name => "Memory Key Store";

        public override KeyStore CreateKeyStore()
        {
            return new MemoryKeyStore();
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new MemoryKeyStoreProperties();
        }

        public override UserControl CreateKeyStorePropertiesControl()
        {
            return new MemoryKeyStorePropertiesControl();
        }

        public override KeyStorePropertiesControlViewModel CreateKeyStorePropertiesControlViewModel()
        {
            return new MemoryKeyStorePropertiesControlViewModel();
        }
    }
}
