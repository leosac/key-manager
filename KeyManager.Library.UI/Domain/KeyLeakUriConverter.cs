using Net.Codecrete.QrCodeGenerator;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyLeakUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not Key key)
            {
                return Binding.DoNothing;
            }

            if (key.KeySize == 0)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8603 // Possible null reference return.
            }

            var checksum = new Sha256Checksum();
            string uri = string.Format("https://leak.leosac.com/key/{0}", checksum.ComputeKCV(key, "53656375726974792046726565646f6d"));
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
