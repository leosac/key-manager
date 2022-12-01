using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyEntryLink : Link
    {
        public KeyEntryLink() : base()
        {
            _wrappingKeySelector = "0";
            _wrappingKeyId = new KeyEntryId();
        }

        private KeyEntryId _wrappingKeyId;

        public KeyEntryId WrappingKeyId
        {
            get => _wrappingKeyId;
            set => SetProperty(ref _wrappingKeyId, value);
        }

        private string _wrappingKeySelector;

        public string WrappingKeySelector
        {
            get => _wrappingKeySelector;
            set => SetProperty(ref _wrappingKeySelector, value);
        }
    }
}
