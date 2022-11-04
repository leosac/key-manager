using Leosac.KeyManager.Library.Policy;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library
{
    public class Key : KMObject
    {
        public Key()
        {
            Policies = new ObservableCollection<IKeyPolicy>();
            _value = String.Empty;
        }

        private string _value;

        public string Value
        {
            get => _value;
            set
            {
                ValidatePolicies(Value);
                SetProperty(ref _value, value);
            }
        }

        public ObservableCollection<IKeyPolicy> Policies { get; set; }

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