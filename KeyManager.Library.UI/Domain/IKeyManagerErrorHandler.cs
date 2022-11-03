using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public interface IKeyManagerErrorHandler
    {
        void HandleError(Exception ex);
    }
}
