using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.KeePass.UI
{
    public class KeePassKeyStoreUIFactory : KeyStoreUIFactory
    {
        public override string Name => "KeePass Key Store";

        public override IDictionary<string, UserControl> CreateKeyStoreAdditionalControls()
        {
            throw new NotImplementedException();
        }

        public override UserControl CreateKeyStorePropertiesControl()
        {
            throw new NotImplementedException();
        }

        public override KeyStorePropertiesControlViewModel? CreateKeyStorePropertiesControlViewModel()
        {
            throw new NotImplementedException();
        }

        public override Type? GetPropertiesType()
        {
            throw new NotImplementedException();
        }
    }
}
