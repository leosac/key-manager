using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class NullToBooleanConverter : IValueConverter
    {
        protected bool NullToBoolean(object parameter)
        {
            if (parameter != null)
            {
                if (parameter is bool v)
                {
                    return v;
                }
            }

            return false;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = NullToBoolean(parameter);
            if (value == null)
                return b;

            if (value is string s)
            {
                if (string.IsNullOrEmpty(s))
                    return b;
            }

            return !b;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = NullToBoolean(parameter);
            if (value is bool v)
            {
                return (v == !b) ? 0 : null;
            }

            return null;
        }
    }
}
