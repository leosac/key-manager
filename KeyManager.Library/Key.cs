using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.Policy;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Text;

namespace Leosac.KeyManager.Library
{
    public class Key : ObservableObject
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

        [JsonIgnore]
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

        public string? GetAggregatedValueAsString()
        {
            return GetAggregatedValueAsString(KeyValueStringFormat.HexString);
        }

        public string? GetAggregatedValueAsString(KeyValueStringFormat format)
        {
            return GetAggregatedValueAsString(format, KeySize == 0 ? Environment.NewLine : null);
        }

        public string? GetAggregatedValueAsString(KeyValueStringFormat format, string? delimiter)
        {
            StringBuilder? ret = null;
            foreach (var m in Materials)
            {
                var v = m.GetValueAsString(format);
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

        public byte[]? GetAggregatedValueAsBinary()
        {
            return GetAggregatedValueAsBinary(false);
        }

        public byte[]? GetAggregatedValueAsBinary(bool padKeySize)
        {
            var data = new List<byte>();
            foreach (var m in Materials)
            {
                var mdata = m.GetValueAsBinary();
                if (mdata != null)
                {
                    data.AddRange(mdata);
                    var padsize = m.OverrideSize > 0 ? m.OverrideSize : KeySize;
                    if (padKeySize && padsize > 0 && mdata.Length < padsize)
                    {
                        data.AddRange(new byte[padsize - mdata.Length]);
                    }
                }
            }
            return data.ToArray();
        }

        public void SetAggregatedValueAsString(string? value)
        {
            SetAggregatedValueAsString(value, KeyValueStringFormat.HexString);
        }

        public void SetAggregatedValueAsString(string? value, KeyValueStringFormat format)
        {
            if (KeySize == 0)
            {
                SetAggregatedValueAsString(value, format, Environment.NewLine);
            }
            else
            {
                var invariant = KeyMaterial.GetInvariantStringValue(value, format);
                if (!string.IsNullOrEmpty(invariant))
                {
                    int i = 0;
                    int pos = 0;
                    do
                    {
                        int length = (int)(Materials[i].OverrideSize > 0 ? Materials[i].OverrideSize : KeySize) * 2;
                        var sub = invariant.Substring(pos, (pos + length) < invariant.Length ? length : (invariant.Length - pos));
                        Materials[i++].SetValueAsString(sub, KeyValueStringFormat.HexString);
                        pos += length;
                    } while (i < Materials.Count && invariant.Length > pos);
                }
            }
        }

        public void SetAggregatedValueAsString(string? value, KeyValueStringFormat format, string? separator)
        {
            var v = value ?? string.Empty;
            var values = separator != null ? v.Split(separator) : new[] { v };

            if (Materials.Count >= values.Length)
            {
                for (int i = 0; i < values.Length; ++i)
                {
                    Materials[i].SetValueAsString(values[i], format);
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