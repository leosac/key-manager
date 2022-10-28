using Leosac.KeyManager.Library.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.File.UI
{
    public class FileKeyStoreFactory : KeyStoreFactory
    {
        public override string Name => "File Key Store";

        public override KeyStore CreateKeyStore()
        {
            return new FileKeyStore();
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new FileKeyStoreProperties();
        }

        public override UserControl CreateKeyStorePropertiesControl()
        {
            return new FileKeyStorePropertiesControl();
        }
    }
}
