using Leosac.KeyManager.Library.Policy;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace Leosac.KeyManager.Library
{
    public class Key : KMObject
    {
        public Key()
        {
            _value = String.Empty;
            Tags = new ObservableCollection<string>();
            Policies = new ObservableCollection<IKeyPolicy>();
            _link = new KeyLink();
        }

        public Key(string[] tags, uint keySize, string value = "")
        {
            Tags = new ObservableCollection<string>(tags);
            _keySize = keySize;
            _value = value;
            Policies = new ObservableCollection<IKeyPolicy>
            {
                new KeyLengthPolicy(keySize)
            };
            _link = new KeyLink();
        }

        private string _value;

        public string Value
        {
            get => _value;
            set
            {
                ValidatePolicies(value);
                SetProperty(ref _value, value);
            }
        }

        public ObservableCollection<string> Tags { get; private set; }

        private uint _keySize;

        public uint KeySize
        {
            get => _keySize;
            set => SetProperty(ref _keySize, value);
        }

        public ObservableCollection<IKeyPolicy> Policies { get; set; }

        private KeyLink _link;

        public KeyLink Link
        {
            get => _link;
            set => SetProperty(ref _link, value);
        }

        public void ValidatePolicies()
        {
            ValidatePolicies(this.Value);
        }

        public void ValidatePolicies(string key)
        {
            foreach (var policy in Policies)
            {
                policy.Validate(key);
            }
        }
    }
}