using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.Policy
{
    public interface IKeyPolicy
    {
        void Validate(string? key);
    }
}
