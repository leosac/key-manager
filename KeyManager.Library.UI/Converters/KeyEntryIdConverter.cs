using Leosac.KeyManager.Library.KeyStore;
using System.Globalization;
using System.Windows.Data;

namespace Leosac.KeyManager.Library.UI.Converters
{
    public class KeyEntryIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value?.ToString() ?? "";

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
                return null;

            if (Guid.TryParse(value.ToString(), out var guid))
                return new KeyEntryId(guid.ToString());

            return null;
        }
    }
}