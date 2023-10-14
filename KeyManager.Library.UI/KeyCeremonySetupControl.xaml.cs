using Leosac.KeyManager.Library.UI.Domain;
using SecretSharingDotNet.Cryptography;
using SecretSharingDotNet.Math;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyCeremonySetupControl.xaml
    /// </summary>
    public partial class KeyCeremonySetupControl : UserControl
    {
        public KeyCeremonySetupControl()
        {
            CeremonyTypes = new ObservableCollection<KeyCeremonyType>(Enum.GetValues<KeyCeremonyType>());

            InitializeComponent();
        }

        public ObservableCollection<KeyCeremonyType> CeremonyTypes { get; set; }

        public KeyCeremonyType SelectedCeremonyType
        {
            get { return (KeyCeremonyType)GetValue(SelectedCeremonyTypeProperty); }
            set { SetValue(SelectedCeremonyTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedCeremonyTypeProperty = DependencyProperty.Register(nameof(SelectedCeremonyType), typeof(KeyCeremonyType), typeof(KeyCeremonySetupControl),
            new FrameworkPropertyMetadata(KeyCeremonyType.ShamirSecretSharing));

        public int Fragments
        {
            get { return (int)GetValue(FragmentsProperty); }
            set { SetValue(FragmentsProperty, value); }
        }

        public static readonly DependencyProperty FragmentsProperty = DependencyProperty.Register(nameof(Fragments), typeof(int), typeof(KeyCeremonySetupControl),
            new FrameworkPropertyMetadata(3));

        public string? KeyValue
        {
            get { return (string)GetValue(KeyValueProperty); }
            set { SetValue(KeyValueProperty, value); }
        }

        public static readonly DependencyProperty KeyValueProperty = DependencyProperty.Register(nameof(KeyValue), typeof(string), typeof(KeyCeremonySetupControl));

        private void BtnCeremony_Click(object sender, RoutedEventArgs e)
        {
            var model = new KeyCeremonyDialogViewModel
            {
                Fragments = new ObservableCollection<string>(new string[Fragments])
            };
            var dialog = new KeyCeremonyDialog
            {
                DataContext = model
            };
            if (dialog.ShowDialog() == true)
            {
                KeyValue = ComputeKeyCeremony(SelectedCeremonyType, model.Fragments.ToArray());
            }
        }

        private static string ComputeKeyCeremony(KeyCeremonyType ceremonyType, string[] fragments)
        {
            string keystr = string.Empty;
            switch (ceremonyType)
            {
                case KeyCeremonyType.Concat:
                    {
                        keystr = String.Join("", fragments);
                    }
                    break;

                case KeyCeremonyType.Xor:
                    {
                        if (fragments.Length > 0)
                        {
                            var key = new byte[fragments[0].Length];
                            foreach (string fragment in fragments)
                            {
                                var keyb = Convert.FromHexString(fragment);
                                for (int i = 0; i < key.Length && i < keyb.Length; ++i)
                                {
                                    key[i] ^= keyb[i];
                                }
                            }
                            keystr = Convert.ToHexString(key);
                        }
                    }
                    break;

                default:
                    {
                        var gcd = new ExtendedEuclideanAlgorithm<BigInteger>();
                        var combine = new ShamirsSecretSharing<BigInteger>(gcd);
                        var shares = String.Join(Environment.NewLine, fragments);
                        var secret = combine.Reconstruction(shares);
                        var key = secret.ToByteArray();
                        keystr = Convert.ToHexString(key);
                    }
                    break;
            }
            return keystr;
        }
    }
}
