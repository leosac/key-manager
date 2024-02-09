using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyEntryCryptogram : ObservableObject, IChangeKeyEntry
    {
        public KeyEntryCryptogram()
        {
            _identifier = new KeyEntryId();
        }

        private KeyEntryId _identifier;

        public KeyEntryId Identifier
        {
            get => _identifier;
            set => SetProperty(ref _identifier, value);
        }

        public KeyEntryClass KClass => KeyEntryClass.Symmetric;

        private string? _value;

        public string? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private WrappingKey? _wrappingKey;

        public WrappingKey? WrappingKey
        {
            get => _wrappingKey;
            set => SetProperty(ref _wrappingKey, value);
        }
    }
}
