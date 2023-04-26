namespace Leosac.KeyManager.Library
{
    public class KeyLink : Link
    {
        public KeyLink() : base()
        {
            
        }

        private string? _containerSelector;

        public string? ContainerSelector
        {
            get => _containerSelector;
            set => SetProperty(ref _containerSelector, value);
        }
    }
}
