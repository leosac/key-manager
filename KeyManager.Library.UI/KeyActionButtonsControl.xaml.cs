using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyActionButtonsControl.xaml
    /// </summary>
    public partial class KeyActionButtonsControl : UserControl
    {
        public KeyActionButtonsControl()
        {
            InitializeComponent();

            Key = new KeyManager.Library.Key();
        }

        public KeyManager.Library.Key Key
        {
            get { return (KeyManager.Library.Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(Key), typeof(KeyManager.Library.Key), typeof(KeyActionButtonsControl),
            new FrameworkPropertyMetadata());

        public bool ShowKeyLink
        {
            get { return (bool)GetValue(ShowKeyLinkProperty); }
            set { SetValue(ShowKeyLinkProperty, value); }
        }

        public static readonly DependencyProperty ShowKeyLinkProperty = DependencyProperty.Register(nameof(ShowKeyLink), typeof(bool), typeof(KeyActionButtonsControl),
            new FrameworkPropertyMetadata(true));

        public KeyEntryClass KClass
        {
            get { return (KeyEntryClass)GetValue(KClassProperty); }
            set { SetValue(KClassProperty, value); }
        }

        public static readonly DependencyProperty KClassProperty = DependencyProperty.Register(nameof(KClass), typeof(KeyEntryClass), typeof(KeyActionButtonsControl),
            new FrameworkPropertyMetadata(KeyEntryClass.Symmetric));

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Key?.GetAggregatedValueAsString());
        }

        private async void BtnKeyStoreLink_Click(object sender, RoutedEventArgs e)
        {
            if (Key != null)
            {
                var model = new KeyLinkDialogViewModel
                {
                    Link = Key.Link,
                    Class = Key.Tags.Contains(nameof(KeyEntryClass.Asymmetric)) ? KeyEntryClass.Asymmetric : KeyEntryClass.Symmetric
                };
                var dialog = new KeyLinkDialog
                {
                    DataContext = model
                };

                await DialogHelper.ForceShow(dialog, "KeyEntryDialog");
            }
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                CheckFileExists = true
            };
            if (ofd.ShowDialog() == true)
            {
                var key = System.IO.File.ReadAllBytes(ofd.FileName);
                Key.SetAggregatedValueAsString(Convert.ToHexString(key));
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == true)
            {
                System.IO.File.WriteAllBytes(sfd.FileName, Convert.FromHexString(Key.GetAggregatedValueAsString() ?? ""));
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            var printDialog = new PrintDialog();
            if (Key != null && printDialog.ShowDialog() == true)
            {
                var control = new KeyPrintControl
                {
                    Key = Key
                };
                if (KClass == KeyEntryClass.Symmetric)
                {
                    var kcv = new KeyGen.KCV();
                    control.KeyChecksum = kcv.ComputeKCV(Key.Tags, Key.GetAggregatedValueAsString() ?? "", null);
                }
                printDialog.PrintVisual(control, "Leosac Key Manager - Key Printing");
            }
        }
    }
}
