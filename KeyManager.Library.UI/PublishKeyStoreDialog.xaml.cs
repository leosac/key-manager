using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for PublishKeyStoreDialog.xaml
    /// </summary>
    public partial class PublishKeyStoreDialog : UserControl
    {
        public PublishKeyStoreDialog()
        {
            InitializeComponent();
        }
    }


    public class KeyEntryIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value?.ToString() ?? "";

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
                return null;

            if (Guid.TryParse(value.ToString(), out var guid))
                return new KeyEntryId(guid.ToString());

            return null;
        }
    }
}
