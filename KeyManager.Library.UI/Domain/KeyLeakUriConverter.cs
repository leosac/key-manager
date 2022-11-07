using Net.Codecrete.QrCodeGenerator;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyLeakUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is Key key))
                return Binding.DoNothing;

            var checksum = new Sha256Checksum();
            string uri = String.Format("https://leak.leosac.com/key/{0}", checksum.ComputeKCV(key));
            if (targetType == typeof(ImageSource))
            {
                var qr = QrCode.EncodeText(uri, QrCode.Ecc.Medium);
                return qr.ToPng(10, 4);
            }
            else
            {
                return uri;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
