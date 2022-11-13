using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyLinkDialogViewModel : ViewModelBase
    {
        public KeyLinkDialogViewModel()
        {

        }

        private KeyLink? _keyLink;

        public KeyLink? KeyLink

        {
            get => _keyLink;
            set => SetProperty(ref _keyLink, value);
        }
    }
}
