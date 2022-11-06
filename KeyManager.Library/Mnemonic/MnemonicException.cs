using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.Mnemonic
{
    public class MnemonicException : Exception
    {
        public MnemonicException() : base()
        {
        }

        public MnemonicException(string message) : base(message)
        {
        }

        public MnemonicException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
