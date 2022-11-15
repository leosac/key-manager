using Leosac.KeyManager.Library.KeyStore.File.UI.Domain;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
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

        public override Type GetKeyStorePropertiesType()
        {
            return typeof(FileKeyStoreProperties);
        }
        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new FileKeyStoreProperties();
        }

        public override UserControl CreateKeyStorePropertiesControl()
        {
            return new FileKeyStorePropertiesControl();
        }

        public override KeyStorePropertiesControlViewModel CreateKeyStorePropertiesControlViewModel()
        {
            return new FileKeyStorePropertiesControlViewModel();
        }

        public override IDictionary<string, UserControl> CreateKeyStoreAdditionalControls()
        {
            return new Dictionary<string, UserControl>();
        }
    }
}
