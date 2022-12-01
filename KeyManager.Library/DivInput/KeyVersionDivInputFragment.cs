using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.DivInput
{
    public class KeyVersionDivInputFragment : DivInputFragment
    {
        public override string GetFragment(DivInputContext context)
        {
            if (context.KeyContainer is KeyVersion kv)
                return kv.Version.ToString("X2");
            return string.Empty;
        }
    }
}
