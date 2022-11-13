using Leosac.KeyManager.Library.DivInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyLink : KMObject
    {
        public KeyLink()
        {
            _keyVersion = 0;
            DivInput = new ObservableCollection<DivInputFragment>();
        }

        private string? _keyStoreFavorite;

        public string? KeyStoreFavorite
        {
            get => _keyStoreFavorite;
            set => SetProperty(ref _keyStoreFavorite, value);
        }

        private string? _keyIdentifier;

        public string? KeyIdentifier
        {
            get => _keyIdentifier;
            set => SetProperty(ref _keyIdentifier, value);
        }

        private int _keyVersion;

        public int KeyVersion
        {
            get => _keyVersion;
            set => SetProperty(ref _keyVersion, value);
        }

        public ObservableCollection<DivInputFragment> DivInput { get; set; }
    }
}
