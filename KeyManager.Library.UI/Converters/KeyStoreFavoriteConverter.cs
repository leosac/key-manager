using System.Globalization;
using System.Windows.Data;

namespace Leosac.KeyManager.Library.UI.Converters
{
    public class KeyStoreFavoriteConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is string v)
            {
                return FavoritesManager.Get(v);
            }

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is Favorite v)
            {
                return v.Identifier;
            }

            return null;
        }
    }
}
