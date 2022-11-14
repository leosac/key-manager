using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.Memory.UI.Domain
{
    public class MemoryKeyEntryPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public MemoryKeyEntryPropertiesControlViewModel()
        {
            _properties = new MemoryKeyEntryProperties();
        }

        public MemoryKeyEntryProperties? MemoryProperties
        {
            get { return Properties as MemoryKeyEntryProperties; }
        }
    }
}
