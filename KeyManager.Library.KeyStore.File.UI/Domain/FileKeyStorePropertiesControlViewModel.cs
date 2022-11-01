using Leosac.KeyManager.Library.UI.Domain;
using Leosac.KeyManager.Library.KeyStore.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Leosac.KeyManager.Library.KeyStore.File.UI.Domain
{
    public class FileKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public FileKeyStorePropertiesControlViewModel()
        {
            _properties = new FileKeyStoreProperties();
        }

        public FileKeyStoreProperties? FileProperties
        {
            get { return Properties as FileKeyStoreProperties; }
        }
    }
}
