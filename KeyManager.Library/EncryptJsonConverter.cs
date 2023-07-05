using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Leosac.KeyManager.Library
{
    public class EncryptJsonConverter : JsonConverter
    {
        private readonly byte[] _encryptionKeyBytes;

        public static string MasterKey { private get; set; } = "changeme";

        public EncryptJsonConverter() : this(null)
        {

        }

        public EncryptJsonConverter(string? encryptionKey)
        {
            if (string.IsNullOrEmpty(encryptionKey))
            {
                encryptionKey = MasterKey;
            }
            using (var sha = SHA256.Create())
            {
                _encryptionKeyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var stringValue = value as string;
            if (string.IsNullOrEmpty(stringValue))
            {
                writer.WriteNull();
                return;
            }
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKeyBytes;
                var data = aes.EncryptCbc(Encoding.UTF8.GetBytes(stringValue), new byte[16], System.Security.Cryptography.PaddingMode.PKCS7);
                writer.WriteValue(Convert.ToBase64String(data));
            }
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
                using (var aes = Aes.Create())
                {
                    aes.Key = _encryptionKeyBytes;
                    var data = aes.DecryptCbc(buffer, new byte[16], System.Security.Cryptography.PaddingMode.PKCS7);
                    return Encoding.UTF8.GetString(data);
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
