using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library
{
    public class KeyContainer : ObservableValidator
    {
        public KeyContainer() : this(null, null)
        {

        }

        public KeyContainer(string? name) : this(name, null)
        {

        }

        public KeyContainer(string? name, Key? key)
        {
            _name = name ?? "Key Container";
            _key = key ?? new Key();
        }

        private string _name;
        private Key _key;

        public Key Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}
