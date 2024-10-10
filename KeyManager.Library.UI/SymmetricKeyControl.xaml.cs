using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for SymmetricKeyControl.xaml
    /// </summary>
    public partial class SymmetricKeyControl : UserControl
    {
        static UIPreferences _uipref;

        static SymmetricKeyControl()
        {
            _uipref = UIPreferences.LoadFromFile(true) ?? new UIPreferences();
        }

        public SymmetricKeyControl()
        {
            InitializeComponent();

            Key = new KeyManager.Library.Key();

            KeyChecksumAlgorithms.Add(new KeyGen.CRC32Checksum());
            KeyChecksumAlgorithms.Add(new KeyGen.KCV());
            KeyChecksumAlgorithms.Add(new KeyGen.Sha256Checksum());

            if (_uipref.DefaultChecksumAlgorithm < KeyChecksumAlgorithms.Count)
            {
                SelectedKeyChecksumAlgorithm = KeyChecksumAlgorithms[_uipref.DefaultChecksumAlgorithm];
            }
        }

        public Key Key
        {
            get { return (Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(Key), typeof(Key), typeof(SymmetricKeyControl),
            new FrameworkPropertyMetadata());

        public KeyGen.KeyChecksum SelectedKeyChecksumAlgorithm
        {
            get { return (KeyGen.KeyChecksum)GetValue(SelectedKeyChecksumAlgorithmProperty); }
            set { SetValue(SelectedKeyChecksumAlgorithmProperty, value); }
        }

        public static readonly DependencyProperty SelectedKeyChecksumAlgorithmProperty = DependencyProperty.Register(nameof(SelectedKeyChecksumAlgorithm), typeof(KeyManager.Library.KeyGen.KeyChecksum), typeof(SymmetricKeyControl));

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

        public ObservableCollection<KeyGen.KeyChecksum> KeyChecksumAlgorithms { get; set; } = new ObservableCollection<KeyGen.KeyChecksum>();

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedKeyChecksumAlgorithm != null)
            {
                var index = KeyChecksumAlgorithms.IndexOf(SelectedKeyChecksumAlgorithm);
                if (index != _uipref.DefaultChecksumAlgorithm)
                {
                    _uipref.DefaultChecksumAlgorithm = index;
                    _uipref.SaveToFile();
                }
            }
        }
    }
}
