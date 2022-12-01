using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.DivInput
{
    public class DivInputContext
    {
        public string? CurrentDivInput { get; set; }

        public KeyStore.KeyStore? KeyStore { get; set; }

        public KeyStore.KeyEntry? KeyEntry { get; set; }

        public KeyContainer? KeyContainer { get; set; }
    }
}
