using Leosac.KeyManager.Library.DivInput;
using Leosac.KeyManager.Library.KeyStore;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library
{
    public class Link : KMObject
    {
        public Link()
        {
            DivInput = new ObservableCollection<DivInputFragment>();
            _keyIdentifier = new KeyEntryId();
        }

        private string? _keyStoreFavorite;

        public string? KeyStoreFavorite
        {
            get => _keyStoreFavorite;
            set => SetProperty(ref _keyStoreFavorite, value);
        }

        private KeyEntryId _keyIdentifier;

        public KeyEntryId KeyIdentifier
        {
            get => _keyIdentifier;
            set => SetProperty(ref _keyIdentifier, value);
        }

        public ObservableCollection<DivInputFragment> DivInput { get; set; }
    }
}
