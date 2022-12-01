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
using Leosac.KeyManager.Library.KeyStore;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for SymmetricKeyControl.xaml
    /// </summary>
    public partial class SymmetricKeyControl : UserControl
    {
        public SymmetricKeyControl()
        {
            InitializeComponent();

            KeyChecksumAlgorithms.Add(new CRC32Checksum());
            KeyChecksumAlgorithms.Add(new KCV());
            KeyChecksumAlgorithms.Add(new Sha256Checksum());
            SelectedKeyChecksumAlgorithm = KeyChecksumAlgorithms[0];
        }

        public KeyManager.Library.Key Key
        {
            get { return (KeyManager.Library.Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(Key), typeof(KeyManager.Library.Key), typeof(SymmetricKeyControl),
            new FrameworkPropertyMetadata(new KeyManager.Library.Key()));

        public KeyManager.Library.KeyChecksum SelectedKeyChecksumAlgorithm
        {
            get { return (KeyManager.Library.KeyChecksum)GetValue(SelectedKeyChecksumAlgorithmProperty); }
            set { SetValue(SelectedKeyChecksumAlgorithmProperty, value); }
        }

        public static readonly DependencyProperty SelectedKeyChecksumAlgorithmProperty = DependencyProperty.Register(nameof(SelectedKeyChecksumAlgorithm), typeof(KeyManager.Library.KeyChecksum), typeof(SymmetricKeyControl));

        public string? KeyChecksumIV
        {
            get { return (string)GetValue(KeyChecksumIVProperty); }
            set { SetValue(KeyChecksumIVProperty, value); }
        }

        public static readonly DependencyProperty KeyChecksumIVProperty = DependencyProperty.Register(nameof(KeyChecksumIV), typeof(string), typeof(SymmetricKeyControl),
            new FrameworkPropertyMetadata(""));

        public bool ShowKCV
        {
            get { return (bool)GetValue(ShowKCVProperty); }
            set { SetValue(ShowKCVProperty, value); }
        }

        public static readonly DependencyProperty ShowKCVProperty = DependencyProperty.Register(nameof(ShowKCV), typeof(bool), typeof(SymmetricKeyControl),
            new FrameworkPropertyMetadata(true));

        public bool ShowKeyLink
        {
            get { return (bool)GetValue(ShowKeyLinkProperty); }
            set { SetValue(ShowKeyLinkProperty, value); }
        }

        public static readonly DependencyProperty ShowKeyLinkProperty = DependencyProperty.Register(nameof(ShowKeyLink), typeof(bool), typeof(SymmetricKeyControl),
            new FrameworkPropertyMetadata(true));

        public ObservableCollection<KeyManager.Library.KeyChecksum> KeyChecksumAlgorithms { get; set; } = new ObservableCollection<KeyChecksum>();
    }
}
