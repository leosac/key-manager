using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyEntryControlViewModel : ViewModelBase
    {
        public KeyEntryControlViewModel()
        {
            
        }

        private KeyEntry? _keyEntry;

        public KeyEntry? KeyEntry
        {
            get => _keyEntry;
            set => SetProperty(ref _keyEntry, value);
        }
    }
}
