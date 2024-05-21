using Leosac.SharedServices;

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
    }
}
