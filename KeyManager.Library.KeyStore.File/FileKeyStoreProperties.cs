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
            _fullpath = String.Empty;
        }

        private string _fullpath;

        public string Fullpath
        {
            get => _fullpath;
            set => SetProperty(ref _fullpath, value);
        }
    }
}
