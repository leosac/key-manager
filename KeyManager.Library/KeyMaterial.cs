using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Leosac.KeyManager.Library
{
    public partial class KeyMaterial : ObservableObject
    {
        public static string PRIVATE_KEY => "Private Key";
        public static string PUBLIC_KEY => "Public Key";

        public KeyMaterial() : this(string.Empty, null)
        {

        }

        public KeyMaterial(string value) : this(value, null)
        {

        }

        public KeyMaterial(string value, string? name)
        {
            _value = value ?? string.Empty;
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

        public static string? GetValueAsString(string? value, KeyValueStringFormat format)
        {
            if (value != null)
            {
                if (format == KeyValueStringFormat.HexStringWithSpace)
                {
                    return HexStringRegex().Replace(value, "$0 ").TrimEnd();
                }
            }
            return value;
        }

        public string? GetValueAsString(KeyValueStringFormat format)
        {
            return GetValueAsString(Value, format);
        }

        public void SetValueAsString(string? value, KeyValueStringFormat format)
        {
            Value = GetInvariantStringValue(value, format);
        }

        public byte[]? GetValueAsBinary()
        {
            if (Value != null)
            {
                return Convert.FromHexString(Value);
            }

            return null;
        }

        public void SetValueAsBinary(byte[]? value)
        {
            Value = (value != null) ? Convert.ToHexString(value) : string.Empty;
        }

        public static string GetInvariantStringValue(string? value, KeyValueStringFormat format)
        {
            return format switch
            {
                KeyValueStringFormat.HexStringWithSpace => (value ?? string.Empty).Replace(" ", ""),
                _ => value ?? string.Empty
            };
        }

        [GeneratedRegex(".{2}")]
        private static partial Regex HexStringRegex();
    }
}
