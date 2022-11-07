using Leosac.KeyManager.Library.UI.Domain;
using Leosac.KeyManager.Library.Policy;
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
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyControl.xaml
    /// </summary>
    public partial class KeyControl : UserControl
    {
        public KeyControl()
        {
            InitializeComponent();

            KeyChecksumAlgorithms.Add(new KCV());
            KeyChecksumAlgorithms.Add(new Sha256Checksum());
            SelectedKeyChecksumAlgorithm = KeyChecksumAlgorithms[0];
        }

        public KeyManager.Library.Key Key
        {
            get { return (KeyManager.Library.Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(Key), typeof(KeyManager.Library.Key), typeof(KeyControl),
            new FrameworkPropertyMetadata(new KeyManager.Library.Key()));

        public KeyManager.Library.KeyChecksum SelectedKeyChecksumAlgorithm
        {
            get { return (KeyManager.Library.KeyChecksum)GetValue(SelectedKeyChecksumAlgorithmProperty); }
            set { SetValue(SelectedKeyChecksumAlgorithmProperty, value); }
        }

        public static readonly DependencyProperty SelectedKeyChecksumAlgorithmProperty = DependencyProperty.Register(nameof(SelectedKeyChecksumAlgorithm), typeof(KeyManager.Library.KeyChecksum), typeof(KeyControl));

        public string? KeyChecksumIV
        {
            get { return (string)GetValue(KeyChecksumIVProperty); }
            set { SetValue(KeyChecksumIVProperty, value); }
        }

        public static readonly DependencyProperty KeyChecksumIVProperty = DependencyProperty.Register(nameof(KeyChecksumIV), typeof(string), typeof(KeyControl),
            new FrameworkPropertyMetadata(""));

        public ObservableCollection<KeyManager.Library.KeyChecksum> KeyChecksumAlgorithms { get; set; } = new ObservableCollection<KeyChecksum>();

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Key?.Value);
        }

        private void btnKeyStoreLink_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == true)
            {
                var key = System.IO.File.ReadAllBytes(ofd.FileName);
                Key.Value = Convert.ToHexString(key);
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == true)
            {
                System.IO.File.WriteAllBytes(sfd.FileName, Convert.FromHexString(Key.Value));
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var control = new KeyPrintControl();
                control.Key = Key;
                control.KeyChecksum = tbxKCV.Text;
                printDialog.PrintVisual(control, "Leosac Key Manager - Key Printing");
            }
        }
    }
}
