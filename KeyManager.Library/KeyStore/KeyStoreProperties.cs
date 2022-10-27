using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public abstract class KeyStoreProperties
    {
        public Key? WrappingKey { get; set; }
    }
}
