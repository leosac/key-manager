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
            return context.KeyVersion!.Version.ToString("X2");
        }
    }
}
