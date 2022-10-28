using Leosac.KeyManager.Library.UI.Domain;
using Leosac.KeyManager.Library.KeyStore.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.File.UI.Domain
{
    public class FileKeyStorePropertiesControlViewModel : ViewModelBase
    {
        public FileKeyStorePropertiesControlViewModel()
        {
            _properties = new FileKeyStoreProperties();
        }

        private FileKeyStoreProperties _properties;

        public FileKeyStoreProperties Properties
        {
            get => _properties;
            set => SetProperty(ref _properties, value);
        }
    }
}
