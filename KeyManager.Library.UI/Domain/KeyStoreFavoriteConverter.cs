using System.Globalization;
using System.Windows.Data;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyStoreFavoriteConverter : IValueConverter
    {
        public KeyStoreFavoriteConverter()
        {
            _favorites = Favorites.GetSingletonInstance();
        }

        private readonly Favorites _favorites;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is string v)
            {
                return _favorites.KeyStores.Where(ks => ks.Name == v).FirstOrDefault();
            }

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is Favorite v)
            {
                return v.Name;
            }

            return null;
        }
    }
}
