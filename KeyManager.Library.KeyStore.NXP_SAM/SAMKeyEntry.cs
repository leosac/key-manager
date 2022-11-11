using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public abstract class SAMKeyEntry : KeyEntry
    {
        public SAMKeyEntry()
        {
            Properties = new SAMKeyEntryProperties();
        }

        public SAMKeyEntryProperties? SAMProperties
        {
            get { return Properties as SAMKeyEntryProperties; }
        }
    }
}
