using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyStoreException : Exception
    {
        public KeyStoreException(string? message) : base(message) { }

        public KeyStoreException(string? message, Exception? exception) : base(message, exception) { }
    }
}
