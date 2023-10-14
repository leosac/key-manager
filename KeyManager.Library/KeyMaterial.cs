using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Leosac.KeyManager.Library
{
    public partial class KeyMaterial : ObservableValidator
    {
        public const string PRIVATE_KEY = "Private Key";
        public const string PUBLIC_KEY = "Public Key";

        public KeyMaterial(string value = "", string? name = null)
        {
            _value = value;
            _name = name;
        }

        [JsonIgnore]
        public EventHandler<string>? BeforeValueChanged { get; set; }

        protected void OnBeforeValueChange(string value)
        {
            BeforeValueChanged?.Invoke(this, value);
        }

        private string _value;

        public string Value
        {
            get => _value;
            set
            {
                OnBeforeValueChange(value);
                SetProperty(ref _value, value);
            }
        }

        private string? _name;

        public string? Name
        {
            get => _name;
            set { SetProperty(ref _name, value); }
        }

        public static T? GetFormattedValue<T>(string? value, KeyValueFormat format) where T : class
        {
            T? v;
            if (value != null)
            {
                switch (format)
                {
                    case KeyValueFormat.HexString:
                        if (typeof(T) != typeof(string))
                            throw new InvalidCastException();
                        v = value as T;
                        break;
                    case KeyValueFormat.HexStringWithSpace:
                        if (typeof(T) != typeof(string))
                            throw new InvalidCastException();
                        v = HexStringRegex().Replace(value, "$0 ") as T;
                        break;
                    case KeyValueFormat.Binary:
                    default:
                        if (typeof(T) != typeof(byte[]))
                            throw new InvalidCastException();
                        v = Convert.FromHexString(value) as T;
                        break;
                }
            }
            else
            {
                v = null;
            }
            return v;
        }

        public T? GetFormattedValue<T>(KeyValueFormat format) where T : class
        {
            return GetFormattedValue<T>(Value, format);
        }

        public void SetFormattedValue(object value, KeyValueFormat format)
        {
            switch (format)
            {
                case KeyValueFormat.HexString:
                case KeyValueFormat.HexStringWithSpace:
                    {
                        var v = value as string;
                        Value = v ?? string.Empty;
                    }
                    break;
                case KeyValueFormat.Binary:
                default:
                    {
                        Value = value is byte[] v ? Convert.ToHexString(v) : string.Empty;
                    }
                    break;
            }
        }

        [GeneratedRegex(".{2}")]
        private static partial Regex HexStringRegex();
    }
}
