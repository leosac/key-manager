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
            set => SetProperty(ref _value, value);
        }

        public ObservableCollection<IKeyPolicy> Policies { get; set; }

        public void ValidatePolicies()
        {
            foreach (var policy in Policies)
            {
                policy.Validate(this);
            }
        }
    }
}