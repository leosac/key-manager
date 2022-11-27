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
            _wrappingKeyVersion = 0;
            _wrappingKeyId = new KeyEntryId();
        }

        private KeyEntryId _wrappingKeyId;

        public KeyEntryId WrappingKeyId
        {
            get => _wrappingKeyId;
            set => SetProperty(ref _wrappingKeyId, value);
        }

        private byte _wrappingKeyVersion;

        public byte WrappingKeyVersion
        {
            get => _wrappingKeyVersion;
            set => SetProperty(ref _wrappingKeyVersion, value);
        }
    }
}
