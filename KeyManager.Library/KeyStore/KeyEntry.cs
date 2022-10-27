using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyEntry
    {
        public string Identifier { get; set; } = new Guid().ToString();
    }
}
