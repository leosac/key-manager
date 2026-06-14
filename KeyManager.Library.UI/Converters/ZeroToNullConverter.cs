using System.Globalization;
using System.Windows.Data;

namespace Leosac.KeyManager.Library.UI.Converters
{
    public sealed class ZeroToNullConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int count && count > 0 ? count : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}