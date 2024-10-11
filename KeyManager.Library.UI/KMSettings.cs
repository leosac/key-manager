using Leosac.SharedServices;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Leosac.KeyManager.Library.UI
{
    public class KMSettings : PermanentConfig<KMSettings>
    {
        private string? _favoritesPath;
        public string? FavoritesPath
        {
            get => _favoritesPath;
            set => SetProperty(ref _favoritesPath, value);
        }

        public string? ElevationCode { get; set; }

        private string? _elevationCodePlain;
        [JsonIgnore]
        public string? ElevationCodePlain
        {
            get => _elevationCodePlain;
            set => SetProperty(ref _elevationCodePlain, value);
        }

        public StoredSecretEncryptionType EncryptionType { get; set; } = StoredSecretEncryptionType.CustomKey;

        public static string? ComputeCodeHash(string? code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }

            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("LKM;" + code)));
        }

        public void Elevate(string code)
        {
            if (!string.IsNullOrEmpty(ElevationCode) && ComputeCodeHash(code) == ElevationCode)
            {
                UIPreferences.IsUserElevated = true;
            }
        }
    }
}
