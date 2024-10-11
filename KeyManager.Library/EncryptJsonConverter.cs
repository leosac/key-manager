using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Leosac.KeyManager.Library
{
    public class EncryptJsonConverter : JsonConverter
    {
        private readonly byte[]? _encryptionKey;
        // SHA256 of "changeme"
        private static readonly string _defaultMasterKey = "057BA03D6C44104863DC7361FE4578965D1887360F90A0895882E58A6248FC86";
        private static string? _globalMasterKey = _defaultMasterKey;
        private static StoredSecretEncryptionType _encryptionType = StoredSecretEncryptionType.CustomKey;

        public EncryptJsonConverter() : this(null) { }

        public EncryptJsonConverter(string? encryptionKey)
        {
            if (string.IsNullOrEmpty(encryptionKey))
            {
                _encryptionKey = null;
            }
        }

        public static StoredSecretEncryptionType EncryptionType
        {
            get => _encryptionType;
        }

        public static bool IsDefaultMasterKey()
        {
            return _globalMasterKey == null || _globalMasterKey.ToLowerInvariant() == _defaultMasterKey.ToLowerInvariant();
        }

        public static void ResetToDefaultMasterKey()
        {
            _globalMasterKey = _defaultMasterKey;
        }

        public static void ChangeEncryption(StoredSecretEncryptionType encryptionType, string? masterKey = null)
        {
            _encryptionType = encryptionType;
            _globalMasterKey = masterKey;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var stringValue = value as string;
            if (string.IsNullOrEmpty(stringValue))
            {
                writer.WriteNull();
                return;
            }

            switch (_encryptionType)
            {
                case StoredSecretEncryptionType.None:
                    writer.WriteValue(stringValue);
                    break;
                case StoredSecretEncryptionType.PerMachine:
                    {
#pragma warning disable CA1416 // Validate platform compatibility
                        var data = ProtectedData.Protect(Encoding.UTF8.GetBytes(stringValue), GetEntropy(), DataProtectionScope.LocalMachine);
#pragma warning restore CA1416 // Validate platform compatibility
                        writer.WriteValue(Convert.ToBase64String(data));
                    }
                    break;
                case StoredSecretEncryptionType.CustomKey:
                default:
                    {
                        using var aes = Aes.Create();
                        aes.Key = GetKey();
                        var data = aes.EncryptCbc(Encoding.UTF8.GetBytes(stringValue), new byte[16], System.Security.Cryptography.PaddingMode.PKCS7);
                        writer.WriteValue(Convert.ToBase64String(data));
                    }
                    break;
            }
        }

        private byte[]? GetEntropy()
        {
            return null;
        }

        private byte[] GetKey()
        {
            return _encryptionKey ?? Convert.FromHexString(_globalMasterKey ?? _defaultMasterKey);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var value = reader.Value as string;
            if (string.IsNullOrEmpty(value))
            {
                return reader.Value;
            }

            try
            {
                switch (_encryptionType)
                {
                    case StoredSecretEncryptionType.None:
                        return value;
                    case StoredSecretEncryptionType.PerMachine:
                        {
                            var buffer = Convert.FromBase64String(value);
#pragma warning disable CA1416 // Validate platform compatibility
                            var data = ProtectedData.Unprotect(buffer, GetEntropy(), DataProtectionScope.LocalMachine);
#pragma warning restore CA1416 // Validate platform compatibility
                            return Encoding.UTF8.GetString(data);
                        }
                    case StoredSecretEncryptionType.CustomKey:
                    default:
                        {
                            var buffer = Convert.FromBase64String(value);
                            using var aes = Aes.Create();
                            aes.Key = GetKey();
                            var data = aes.DecryptCbc(buffer, new byte[16], System.Security.Cryptography.PaddingMode.PKCS7);
                            return Encoding.UTF8.GetString(data);
                        }
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}
