using Leosac.KeyManager.Library.DivInput;
using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library
{
    public class KeyLink : Link
    {
        public KeyLink() : base()
        {
            _keyVersion = 0;
        }

        private byte _keyVersion;

        public byte KeyVersion
        {
            get => _keyVersion;
            set => SetProperty(ref _keyVersion, value);
        }
    }
}
