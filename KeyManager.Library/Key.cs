using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.Policy;
using System.Collections.ObjectModel;
using System.Text;

namespace Leosac.KeyManager.Library
{
    public class Key : ObservableValidator
    {
        public Key() : this(null, 0, 1)
        {

        }

        public Key(IEnumerable<string>? tags) : this(tags, 0, 1)
        {

        }

        public Key(IEnumerable<string>? tags, uint keySize) : this(tags, keySize, 1)
        {

        }

        public Key(IEnumerable<string>? tags, uint keySize, uint nbMaterials)
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
                        k.BeforeValueChanged += (_, e) => ValidatePolicies(e);
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

        public string? GetAggregatedValueString()
        {
            return GetAggregatedValueString(KeyValueStringFormat.HexString);
        }

        public string? GetAggregatedValueString(KeyValueStringFormat format)
        {
            return GetAggregatedValueString(format, KeySize == 0 ? Environment.NewLine : null);
        }

        public string? GetAggregatedValueString(KeyValueStringFormat format, string? delimiter)
        {
            StringBuilder? ret = null;
            foreach (var m in Materials)
            {
                var v = m.GetValueString(format);
                if (ret != null)
                {
                    if (delimiter != null)
                    {
                        ret.Append(delimiter);
                    }
                    if (v != null)
                    {
                        ret.Append(v);
                    }
                }
                else
                {
                    ret = new StringBuilder(v ?? string.Empty);
                }
            }
            return ret?.ToString();
        }

        public byte[]? GetAggregatedValueBinary()
        {
            return GetAggregatedValueBinary(false);
        }

        public byte[]? GetAggregatedValueBinary(bool padKeySize)
        {
            var data = new List<byte>();
            foreach (var m in Materials)
            {
                var mdata = m.GetValueBinary();
                if (mdata != null)
                {
                    data.AddRange(mdata);
                }
            }
            if (padKeySize && KeySize > 0 && data.Count < KeySize)
            {
                data.AddRange(new byte[KeySize - data.Count]);
            }
            return data.ToArray();
        }

        public void SetAggregatedValueString(string? value)
        {
            SetAggregatedValueString(value, KeyValueStringFormat.HexString);
        }

        public void SetAggregatedValueString(string? value, KeyValueStringFormat format)
        {
            if (KeySize == 0)
            {
                SetAggregatedValueString(value, format, Environment.NewLine);
            }
            else
            {
                var invariant = KeyMaterial.GetInvariantStringValue(value, format);
                if (!string.IsNullOrEmpty(invariant))
                {
                    int length = (int)KeySize * 2;
                    int i = 0;
                    do
                    {
                        int pos = i * length;
                        var sub = invariant.Substring(pos, (pos + length) < invariant.Length ? length : (invariant.Length - pos));
                        Materials[i++].SetValueString(sub, KeyValueStringFormat.HexString);
                    } while (i < Materials.Count && invariant.Length >= (i + 1) * KeySize * 2);
                }
            }
        }

        public void SetAggregatedValueString(string? value, KeyValueStringFormat format, string? separator)
        {
            var v = value ?? string.Empty;
            var values = separator != null ? v.Split(separator) : new[] { v };

            if (Materials.Count >= values.Length)
            {
                for (int i = 0; i < values.Length; ++i)
                {
                    Materials[i].SetValueString(values[i], format);
                }
            }
        }

        public bool IsEmpty()
        {
            foreach (var m in Materials)
            {
                if (!string.IsNullOrEmpty(m.Value))
                {
                    return false;
                }
            }
            return true;
        }
    }
}