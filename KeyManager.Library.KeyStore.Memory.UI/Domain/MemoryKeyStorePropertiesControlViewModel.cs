using Leosac.KeyManager.Library.UI.Domain;
using Leosac.KeyManager.Library.KeyStore.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.Memory.UI.Domain
{
    public class MemoryKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public MemoryKeyStorePropertiesControlViewModel()
        {
            _properties = new MemoryKeyStoreProperties();
        }
    }
}
