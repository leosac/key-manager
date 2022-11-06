using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library
{
    public static class KeyHelper
    {
        public static uint GetBlockSize(KeyTag tags)
        {
            uint size = 8;
            if ((tags & KeyTag.AES) == KeyTag.AES)
            {
                size = 16;
            }
            return size;
        }
    }
}
