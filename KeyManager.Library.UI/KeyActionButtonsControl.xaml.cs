using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for KeyActionButtonsControl.xaml
    /// </summary>
    public partial class KeyActionButtonsControl : UserControl
    {
        public KeyActionButtonsControl()
        {
            InitializeComponent();
        }

        public KeyManager.Library.Key Key
        {
            get { return (KeyManager.Library.Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(Key), typeof(KeyManager.Library.Key), typeof(KeyActionButtonsControl),
            new FrameworkPropertyMetadata(new KeyManager.Library.Key()));

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

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Key?.GetAggregatedValue<string>());
        }

        private async void btnKeyStoreLink_Click(object sender, RoutedEventArgs e)
        {
            if (Key != null)
            {
                var model = new KeyLinkDialogViewModel()
                {
                    Link = Key.Link,
                    Class = Key.Tags.Contains(nameof(KeyEntryClass.Asymmetric)) ? KeyEntryClass.Asymmetric : KeyEntryClass.Symmetric
                };
                var dialog = new KeyLinkDialog()
                {
                    DataContext = model
                };

                await DialogHost.Show(dialog, "KeyEntryDialog");
            }
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == true)
            {
                var key = System.IO.File.ReadAllBytes(ofd.FileName);
                Key.SetAggregatedValue(Convert.ToHexString(key));
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == true)
            {
                System.IO.File.WriteAllBytes(sfd.FileName, Convert.FromHexString(Key.GetAggregatedValue<string>() ?? ""));
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (Key != null && printDialog.ShowDialog() == true)
            {
                var control = new KeyPrintControl();
                control.Key = Key;
                if (KClass == KeyEntryClass.Symmetric)
                {
                    var kcv = new KCV();
                    control.KeyChecksum = kcv.ComputeKCV(Key.Tags, Key.GetAggregatedValue<string>() ?? "");
                }
                printDialog.PrintVisual(control, "Leosac Key Manager - Key Printing");
            }
        }
    }
}
