using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.File
{
    public class FileKeyStoreProperties : KeyStoreProperties
    {
        public FileKeyStoreProperties() : base()
        {
            _directory = String.Empty;
        }

        private string _directory;

        public string Directory
        {
            get => _directory;
            set => SetProperty(ref _directory, value);
        }
    }
}
