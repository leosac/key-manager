using Leosac.KeyManager.Library.UI.Domain;
using Leosac.KeyManager.Library.KeyStore.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.Memory.UI.Domain
{
    public class MemoryKeyStorePropertiesControlViewModel : ViewModelBase
    {
        public MemoryKeyStorePropertiesControlViewModel()
        {
            _properties = new MemoryKeyStoreProperties();
        }

        private MemoryKeyStoreProperties _properties;

        public MemoryKeyStoreProperties Properties
        {
            get => _properties;
            set => SetProperty(ref _properties, value);
        }
    }
}
