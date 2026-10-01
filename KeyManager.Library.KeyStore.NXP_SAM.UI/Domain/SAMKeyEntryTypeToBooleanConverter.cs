using System.Globalization;
using System.Windows.Data;
using Leosac.KeyManager.Library.KeyStore.NXP_SAM;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain
{
    public sealed class SAMKeyEntryTypeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is SAMKeyEntryType keyEntryType
                && parameter is SAMKeyEntryType expectedKeyEntryType
                && keyEntryType == expectedKeyEntryType;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
