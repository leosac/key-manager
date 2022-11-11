using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public enum SAMKeyEntryType : byte
    {
        Host = 0x00,
        PICC = 0x01,
        OfflineChange = 0x02,
        OfflineCrypto = 0x04
    }
}
