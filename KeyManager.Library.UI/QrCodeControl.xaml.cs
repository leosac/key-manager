using Net.Codecrete.QrCodeGenerator;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for QrCodeControl.xaml
    /// </summary>
    public partial class QrCodeControl : UserControl
    {
        public QrCodeControl()
        {
            InitializeComponent();
        }

        public byte[]? QrCode
        {
            get { return (byte[]?)GetValue(QrCodeProperty); }
            set { SetValue(QrCodeProperty, value); }
        }

        public static readonly DependencyProperty QrCodeProperty = DependencyProperty.Register(nameof(QrCode), typeof(byte[]), typeof(QrCodeControl),
            new FrameworkPropertyMetadata());

        public void GenerateQrCode(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                QrCode = null;
            }
            else
            {
                var qr = Net.Codecrete.QrCodeGenerator.QrCode.EncodeText(value, Net.Codecrete.QrCodeGenerator.QrCode.Ecc.Medium);
                QrCode = qr.ToPng(15, 4);
            }
        }
    }
}
