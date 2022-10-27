using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.Policy
{
    public class KeyPolicyException : Exception
    {
        public KeyPolicyException(string? message) : base(message) { }

        public KeyPolicyException(string? message, Exception? exception) : base(message, exception) { }
    }
}
