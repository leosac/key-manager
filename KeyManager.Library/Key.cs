using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.Policy;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library
{
    public class Key : ObservableValidator
    {
        public Key() : this(null, 0, 1)
        {

        }

        public Key(IEnumerable<string>? tags, uint keySize = 0, uint nbMaterials = 1)
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
            Tags = new ObservableCollection<string>(tags ?? Array.Empty<string>());
            _keySize = keySize;
            Policies = new ObservableCollection<IKeyPolicy>();
            if (keySize > 0)
            {
                Policies.Add(new KeyLengthPolicy(keySize));
            }
            _link = new KeyLink();
        }

        public Key(IEnumerable<string>? tags, string value) : this(tags, 0, value)
        {
        }

        public Key(IEnumerable<string>? tags, uint keySize, string value) : this(tags, keySize, 0)
        {
            Materials.Add(new KeyMaterial(value));
            if (keySize == 0)
            {
                KeySize = (uint)Materials[0].GetFormattedValue<byte[]>(KeyValueFormat.Binary)!.Length;
            }
        }

        public Key(IEnumerable<string>? tags, uint keySize, params KeyMaterial[] materials) : this(tags, keySize, 0)
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

        public T? GetAggregatedValue<T>(KeyValueFormat format = KeyValueFormat.HexString) where T : class
        {
            T? ret = default;
            foreach (var m in Materials)
            {
                var v = m.GetFormattedValue<T>(format);
                if (ret == default(T))
                {
                    ret = v;
                }
                else
                {
                    if (typeof(T) == typeof(byte[]))
                    {
                        ret = (ret as byte[])!.Concat((v as byte[])!) as T;
                    }
                    else if (typeof(T) == typeof(string))
                    {
                        ret = (ret.ToString() + Environment.NewLine + ret.ToString()) as T;
                    }
                }
            }
            return ret;
        }

        public void SetAggregatedValue(object? value)
        {
            SetAggregatedValue(value, KeyValueFormat.HexString);
        }

        public void SetAggregatedValue(object? value, KeyValueFormat format)
        {
            value ??= string.Empty;

            switch(format)
            {
                case KeyValueFormat.HexString:
                    {
                        if (value is string v)
                        {
                            var values = v.Split(Environment.NewLine);
                            if (Materials.Count >= values.Length)
                            {
                                for (int i = 0; i < values.Length; ++i)
                                {
                                    Materials[i].SetFormattedValue(values[i], format);
                                }
                            }
                        }
                    }
                    break;
                case KeyValueFormat.Binary:
                default:
                    if (Materials.Count > 0)
                        Materials[0].SetFormattedValue(value, format);
                    break;
            }
        }
    }
}