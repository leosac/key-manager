using Leosac.KeyManager.Library.UI.Helpers;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyMaterialActionButtonsControl.xaml
    /// </summary>
    public partial class KeyMaterialActionButtonsControl : UserControl
    {
        public KeyMaterialActionButtonsControl()
        {
            InitializeComponent();
        }

        public KeyManager.Library.KeyMaterial KeyMaterial
        {
            get { return (KeyManager.Library.KeyMaterial)GetValue(KeyMaterialProperty); }
            set { SetValue(KeyMaterialProperty, value); }
        }

        public static readonly DependencyProperty KeyMaterialProperty = DependencyProperty.Register(nameof(KeyMaterial), typeof(KeyManager.Library.KeyMaterial), typeof(KeyMaterialActionButtonsControl),
            new FrameworkPropertyMetadata());

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(KeyMaterial?.GetValueAsString(KeyMaterial.ValueFormat) ?? string.Empty);
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            if (KeyMaterial == null)
            {
                return;
            }

            KeyFileDialogHelper.Import(
                value => KeyMaterial.SetValueAsBinary(value),
                (value, format) => KeyMaterial.SetValueAsString(value, format ?? KeyMaterial.ValueFormat),
                (privateKey, publicKey) => KeyMaterial.SetValueAsString(
                    KeyMaterial.Name == KeyManager.Library.KeyMaterial.PRIVATE_KEY ? privateKey ?? publicKey : publicKey ?? privateKey,
                    KeyValueStringFormat.Pem));
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (KeyMaterial == null)
            {
                return;
            }

            KeyFileDialogHelper.Export(
                () => KeyMaterial.GetValueAsBinary() ?? [],
                format => KeyMaterial.GetValueAsString(format ?? KeyMaterial.ValueFormat) ?? string.Empty,
                password => KeyFileDialogHelper.CreatePkcs12(KeyMaterial.GetValueAsString(KeyValueStringFormat.Pem) ?? string.Empty, password));
        }
    }
}
