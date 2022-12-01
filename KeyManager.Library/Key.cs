using Leosac.KeyManager.Library.Policy;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace Leosac.KeyManager.Library
{
    public class Key : KMObject
    {
        public Key() : this(null, 0, 1)
        {

        }

        public Key(string[]? tags, uint keySize = 0, uint nbMaterials = 1)
        {
            Materials = new ObservableCollection<KeyMaterial>();
            Materials.CollectionChanged += Materials_CollectionChanged;
            if (nbMaterials > 0)
            {
                for (uint i = 0; i< nbMaterials; ++i)
                {
                    Materials.Add(new KeyMaterial());
                }
            }
            Tags = new ObservableCollection<string>(tags ?? new string[0]);
            _keySize = keySize;
            Policies = new ObservableCollection<IKeyPolicy>();
            if (keySize > 0)
                Policies.Add(new KeyLengthPolicy(keySize));
            _link = new KeyLink();
        }

        public Key(string[]? tags, uint keySize, string value) : this(tags, keySize, 0)
        {
            Materials.Add(new KeyMaterial(value));
        }

        public Key(string[]? tags, uint keySize, params KeyMaterial[] materials) : this(tags, keySize, 0)
        {
            foreach(var material in materials)
            {
                Materials.Add(material);
            }
        }

        private void Materials_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is KeyMaterial k)
                    {
                        k.BeforeValueChanged += (sender, e) =>
                        {
                            ValidatePolicies(e);
                        };
                    }
                }
            }
        }

        private uint _keySize;

        public uint KeySize
        {
            get => _keySize;
            set => SetProperty(ref _keySize, value);
        }

        public ObservableCollection<KeyMaterial> Materials { get; set; }

        public ObservableCollection<string> Tags { get; set; }

        public ObservableCollection<IKeyPolicy> Policies { get; set; }

        private KeyLink _link;

        public KeyLink Link
        {
            get => _link;
            set => SetProperty(ref _link, value);
        }

        public void ValidatePolicies()
        {
            foreach (var policy in Policies)
            {
                policy.Validate(this);
            }
        }

        public void ValidatePolicies(string value)
        {
            foreach (var policy in Policies)
            {
                policy.Validate(value);
            }
        }

        public string GetAggregatedValue()
        {
            string ret = string.Empty;

            if (Materials.Count > 0)
                ret = Materials[0].Value;

            return ret;
        }

        public void SetAggregatedValue(string? value)
        {
            if (value == null)
                value = string.Empty;

            if (Materials.Count > 0)
                Materials[0].Value = value;
        }
    }
}