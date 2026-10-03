using System.Globalization;
using System.Windows.Data;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class BitLengthToCharLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not uint bitLength)
            {
                return Binding.DoNothing;
            }

            return bitLength / 4;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int characterLength)
            {
                return Binding.DoNothing;
            }

            return characterLength * 4u;
        }
    }
}
