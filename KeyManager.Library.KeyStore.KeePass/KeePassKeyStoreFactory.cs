using Leosac.KeyManager.Library.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.KeePass
{
    public class KeePassKeyStoreFactory : KeyStoreFactory
    {
        public override string Name => "KeePass Key Store";

        public override KeyStore CreateKeyStore()
        {
            throw new NotImplementedException();
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            throw new NotImplementedException();
        }

        public override KeyStoreProperties? CreateKeyStoreProperties(string serialized)
        {
            throw new NotImplementedException();
        }

        public override Type? GetPropertiesType()
        {
            throw new NotImplementedException();
        }
    }
}
