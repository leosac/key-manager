using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyChecksumConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 || !(values[0] is KeyChecksum value1) || !(values[1] is Key value2))
                return Binding.DoNothing;

            if (string.IsNullOrEmpty(value2.GetAggregatedValue()))
                return Binding.DoNothing;

            return value1.ComputeKCV(value2, values.Length >= 2 ? values[2] as string : null );
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
