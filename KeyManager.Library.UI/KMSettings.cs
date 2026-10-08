using Leosac.SharedServices;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Leosac.KeyManager.Library.UI
{
    public class KMSettings : PermanentConfig<KMSettings>
    {
        private string? _favoritesPath;
        /// <summary>
        /// Path to the shared favorites file. If null, the default path will be used.
        /// </summary>
        public string? FavoritesPath
        {
            get => _favoritesPath;
            set => SetProperty(ref _favoritesPath, value);
        }

        public StoredSecretEncryptionType EncryptionType { get; set; } = StoredSecretEncryptionType.CustomKey;

        private bool _userRolesEnabled;
        public bool UserRolesEnabled
        {
            get => _userRolesEnabled;
            set => SetProperty(ref _userRolesEnabled, value);
        }

        private string? _administratorsGroup;
        public string? AdministratorsGroup
        {
            get => _administratorsGroup;
            set => SetProperty(ref _administratorsGroup, value);
        }

        private string? _usersGroup;
        public string? UsersGroup
        {
            get => _usersGroup;
            set => SetProperty(ref _usersGroup, value);
        }

        private string? _defaultFavoriteLink;
        public string? DefaultFavoriteLink
        {
            get => _defaultFavoriteLink;
            set => SetProperty(ref _defaultFavoriteLink, value);
        }

        public static string? ComputeCodeHash(string? code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }

            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("LKM;" + code)));
        }
    }
}
