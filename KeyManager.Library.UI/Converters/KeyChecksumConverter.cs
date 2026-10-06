using System.Globalization;
using System.Windows.Data;

namespace Leosac.KeyManager.Library.UI.Converters
{
    public class KeyChecksumConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 || values[0] is not KeyGen.KeyChecksum value1 || values[1] is not Key value2)
            {
                return Binding.DoNothing;
            }

            if (value2.KeySize == 0)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(value2.GetAggregatedValueAsString()))
            {
                return Binding.DoNothing;
            }

            try
            {
                return value1.ComputeKCV(value2, values.Length >= 2 ? values[2] as string : null);
            }
            catch(Exception)
            {
                return string.Empty;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
