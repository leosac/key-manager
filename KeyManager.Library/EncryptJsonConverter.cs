using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Leosac.KeyManager.Library
{
    public class EncryptJsonConverter : JsonConverter
    {
        private readonly byte[]? _encryptionKey;

        private static readonly string _defaultMasterKey = "057BA03D6C44104863DC7361FE4578965D1887360F90A0895882E58A6248FC86"; // SHA256 of "changeme" text

        public static string MasterKey { private get; set; } = _defaultMasterKey;

        public EncryptJsonConverter() : this(null) { }

        public EncryptJsonConverter(string? encryptionKey)
        {
            if (string.IsNullOrEmpty(encryptionKey))
            {
                _encryptionKey = null;
            }
        }

        public static bool IsDefaultMasterKey()
        {
            return MasterKey.ToLowerInvariant() == _defaultMasterKey.ToLowerInvariant();
        }

        public static void ResetToDefaultMasterKey()
        {
            MasterKey = _defaultMasterKey;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var stringValue = value as string;
            if (string.IsNullOrEmpty(stringValue))
            {
                writer.WriteNull();
                return;
            }
            using var aes = Aes.Create();
            aes.Key = GetKey();
            var data = aes.EncryptCbc(Encoding.UTF8.GetBytes(stringValue), new byte[16], System.Security.Cryptography.PaddingMode.PKCS7);
            writer.WriteValue(Convert.ToBase64String(data));
        }

        private byte[] GetKey()
        {
            return _encryptionKey ?? Convert.FromHexString(MasterKey);
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
                var buffer = Convert.FromBase64String(value);
                using var aes = Aes.Create();
                aes.Key = GetKey();
                var data = aes.DecryptCbc(buffer, new byte[16], System.Security.Cryptography.PaddingMode.PKCS7);
                return Encoding.UTF8.GetString(data);
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
