using Microsoft.Win32;
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

            var ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "Binary Files (*.bin)|*.bin|Text Files (*.txt)|*.txt|PEM Files (*.pem)|*.pem"
            };
            if (ofd.ShowDialog() == true)
            {
                if (ofd.FilterIndex == 1)
                {
                    KeyMaterial?.SetValueAsBinary(System.IO.File.ReadAllBytes(ofd.FileName));
                }
                else
                {
                    KeyMaterial?.SetValueAsString(System.IO.File.ReadAllText(ofd.FileName), ofd.FilterIndex == 3 ? KeyValueStringFormat.Pem : KeyMaterial.ValueFormat);
                }
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (KeyMaterial == null)
            {
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Binary Files (*.bin)|*.bin|Text Files (*.txt)|*.txt|PEM Files (*.pem)|*.pem"
            };
            if (sfd.ShowDialog() == true)
            {
                if (sfd.FilterIndex == 1)
                {
                    System.IO.File.WriteAllBytes(sfd.FileName, KeyMaterial?.GetValueAsBinary() ?? []);
                }
                else
                {
                    System.IO.File.WriteAllText(sfd.FileName, KeyMaterial?.GetValueAsString(sfd.FilterIndex == 3 ? KeyValueStringFormat.Pem : KeyMaterial.ValueFormat) ?? string.Empty);
                }
            }
        }
    }
}
