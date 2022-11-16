using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public interface IChangeKeyEntry
    {
        string Identifier { get; set; }
    }
}
