using System.Globalization;
using System.Windows.Data;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain
{
    public class DESFireKeyTypeConverter : IValueConverter
    {
        public static string GetKeyTypeName(LibLogicalAccess.Card.DESFireKeyType keyType)
        {
            return keyType switch
            {
                LibLogicalAccess.Card.DESFireKeyType.DF_KEY_DES => "DES",
                LibLogicalAccess.Card.DESFireKeyType.DF_KEY_3K3DES => "TK3DES",
                _ => "AES128",
            };
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LibLogicalAccess.Card.DESFireKeyType kt)
            {
                return GetKeyTypeName(kt);
            }
            return string.Empty;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
