using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.DivInput
{
    public abstract class DivInputFragment : KMObject
    {
        public abstract string GetFragment(DivInputContext context);
    }
}
