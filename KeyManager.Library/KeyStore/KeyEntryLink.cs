namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyEntryLink : Link
    {
        public KeyEntryLink()
        {
            _wrappingKey = new WrappingKey();
        }

        private WrappingKey _wrappingKey;

        public WrappingKey WrappingKey
        {
            get => _wrappingKey;
            set => SetProperty(ref _wrappingKey, value);
        }
    }
}
