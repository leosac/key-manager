namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyEntryLink : Link
    {
        public KeyEntryLink()
        {
            _wrappingKeySelector = "0";
            _wrappingKeyId = new KeyEntryId(string.Empty);
        }

        private KeyEntryId _wrappingKeyId;

        public KeyEntryId WrappingKeyId
        {
            get => _wrappingKeyId;
            set => SetProperty(ref _wrappingKeyId, value);
        }

        private string _wrappingKeySelector;

        public string WrappingKeySelector
        {
            get => _wrappingKeySelector;
            set => SetProperty(ref _wrappingKeySelector, value);
        }
    }
}
