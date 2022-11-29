using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public interface IKeyEntry : IChangeKeyEntry
    {
        KeyEntryProperties? Properties { get; set; }

        KeyEntryVariant? Variant { get; set; }

        KeyEntryLink? Link { get; set; }
    }
}
