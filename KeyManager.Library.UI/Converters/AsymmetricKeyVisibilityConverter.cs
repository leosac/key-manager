using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.KeyStore;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Leosac.KeyManager.Library.UI.Converters
{
    public sealed class AsymmetricKeyVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var key = value as Key;
            return key?.Tags.Contains(nameof(KeyEntryClass.Asymmetric)) == true
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}