using System.Globalization;
using System.Windows.Data;

namespace Leosac.KeyManager.Library.UI.Converters
{
    public class HexStringLengthConverter : IValueConverter
    {
        public bool LengthInBits { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!TryConvertToUInt32(value, out var length))
            {
                return Binding.DoNothing;
            }

            return LengthInBits ? length * 4 : length / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!TryConvertToUInt32(value, out var length))
            {
                return Binding.DoNothing;
            }

            return LengthInBits ? length / 4 : length * 2;
        }

        private static bool TryConvertToUInt32(object value, out uint result)
        {
            if (value is not IConvertible convertible || !IsNumeric(value))
            {
                result = default;
                return false;
            }

            try
            {
                result = convertible.ToUInt32(CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        private static bool IsNumeric(object value)
        {
            return Type.GetTypeCode(value.GetType()) is TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.Decimal or TypeCode.Double or TypeCode.Single;
        }
    }
}
