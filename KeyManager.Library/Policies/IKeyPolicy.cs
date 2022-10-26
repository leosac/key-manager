using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.Policies
{
    public interface IKeyPolicy
    {
        void Validate(Key key);
    }
}
