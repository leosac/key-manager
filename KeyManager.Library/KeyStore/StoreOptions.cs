using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class StoreOptions : ObservableObject
    {
        public StoreOptions()
        {
            _wrappingKey = new WrappingKey();
        }

        private WrappingKey _wrappingKey;
        public WrappingKey WrappingKey
        {
            get => _wrappingKey;
            set => SetProperty(ref _wrappingKey, value);
        }

        private bool _generateKeys;
        public bool GenerateKeys
        {
            get => _generateKeys;
            set => SetProperty(ref _generateKeys, value);
        }
    }
}
