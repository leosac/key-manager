using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class WrappingKey : ObservableObject
    {
        public WrappingKey()
        {
            _keyId = new KeyEntryId();
            _containerSelector = "0";
        }

        private KeyEntryId _keyId;
        public KeyEntryId KeyId
        {
            get => _keyId;
            set => SetProperty(ref _keyId, value);
        }

        private string? _containerSelector;
        public string? ContainerSelector
        {
            get => _containerSelector;
            set => SetProperty(ref _containerSelector, value);
        }
    }
}
