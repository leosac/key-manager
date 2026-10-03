using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Security.Cryptography;
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

        public KeyMaterial(string value, string? name) : this(value, name, 0)
        {

        }

        public KeyMaterial(string value, string? name, uint overrideSize)
        {
            _value = value ?? string.Empty;
            _name = name;
            _overrideSize = overrideSize;
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

        private KeyValueStringFormat _valueFormat = KeyValueStringFormat.HexString;
        public KeyValueStringFormat ValueFormat
        {
            get => _valueFormat;
            set => SetProperty(ref _valueFormat, value);
        }

        private string? _name;

        public string? Name
        {
            get => _name;
            set { SetProperty(ref _name, value); }
        }

        private uint _overrideSize;

        public uint OverrideSize
        {
            get => _overrideSize;
            set { SetProperty(ref _overrideSize, value); }
        }

        [JsonIgnore]
        public uint OverrideSizeInBytes => OverrideSize == 0 ? 0 : KeyGeneration.BitsToBytes(OverrideSize);

        [OnDeserialized]
        private void MigrateLegacyByteOverrideSize(StreamingContext context)
        {
            if (OverrideSize is > 0 and <= 4)
            {
                OverrideSize *= 8;
            }
        }

        private bool _validatePolicies = true;
        [JsonIgnore]
        public bool ValidatePolicies
        {
            get => _validatePolicies;
            set { SetProperty(ref _validatePolicies, value); }
        }

        public static string? ConvertValueFormat(string? value, KeyValueStringFormat format)
        {
            return ConvertValueFormat(value, format, KeyValueStringFormat.HexString);
        }

        public static string? ConvertValueFormat(string? value, KeyValueStringFormat outputFormat, KeyValueStringFormat currentFormat)
        {
            if (value == null)
            {
                return null;
            }

            var binaryValue = currentFormat switch
            {
                KeyValueStringFormat.HexString or KeyValueStringFormat.HexStringWithSpace => Convert.FromHexString(value.Replace(" ", string.Empty)),
                KeyValueStringFormat.Der => Convert.FromBase64String(value),
                KeyValueStringFormat.Pem => ConvertPemToBinary(value),
                _ => throw new ArgumentOutOfRangeException(nameof(currentFormat), currentFormat, null)
            };

            return outputFormat switch
            {
                KeyValueStringFormat.HexString => Convert.ToHexString(binaryValue),
                KeyValueStringFormat.HexStringWithSpace => HexStringRegex().Replace(Convert.ToHexString(binaryValue), "$0 ").TrimEnd(),
                KeyValueStringFormat.Der => Convert.ToBase64String(binaryValue),
                KeyValueStringFormat.Pem => ConvertBinaryToPem(binaryValue),
                _ => throw new ArgumentOutOfRangeException(nameof(outputFormat), outputFormat, null)
            };
        }

        public string? GetValueAsString(KeyValueStringFormat format)
        {
            return ConvertValueFormat(Value, format, ValueFormat);
        }

        public void SetValueAsString(string? value, KeyValueStringFormat format)
        {
            Value = ConvertValueFormat(value, KeyValueStringFormat.HexString, format) ?? string.Empty;
        }

        public byte[]? GetValueAsBinary()
        {
            if (Value != null)
            {
                return Convert.FromHexString(ConvertValueFormat(Value, KeyValueStringFormat.HexString, ValueFormat) ?? string.Empty);
            }

            return null;
        }

        public void SetValueAsBinary(byte[]? value)
        {
            SetValueAsString((value != null) ? Convert.ToHexString(value) : string.Empty, KeyValueStringFormat.HexString);
        }

        private static byte[] ConvertPemToBinary(string value)
        {
            var lines = value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (lines.Length < 3 || !lines[0].StartsWith("-----BEGIN ") || !lines[^1].StartsWith("-----END "))
            {
                throw new FormatException("The PEM value must contain a complete PEM block.");
            }

            var beginLabel = lines[0][11..^5];
            var endLabel = lines[^1][9..^5];
            if (!string.Equals(beginLabel, endLabel, StringComparison.Ordinal))
            {
                throw new FormatException("The PEM begin and end labels must match.");
            }

            return Convert.FromBase64String(string.Concat(lines[1..^1]));
        }

        private static string ConvertBinaryToPem(byte[] value)
        {
            var base64 = Convert.ToBase64String(value);
            var lines = Enumerable.Range(0, (base64.Length + 63) / 64)
                .Select(i => base64.Substring(i * 64, Math.Min(64, base64.Length - i * 64)));
            return $"-----BEGIN KEY-----\n{string.Join("\n", lines)}\n-----END KEY-----";
        }

        [GeneratedRegex(".{2}")]
        private static partial Regex HexStringRegex();

        /// <summary>
        /// Some key materials shouldn't be treated as a key... (eg. MIFARE key entries).
        /// </summary>
        public bool IsRealKeyMateriel
        {
            get { return OverrideSize == 0; }
        }
    }
}
