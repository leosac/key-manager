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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

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

        private void BtnUnionCeremony_Click(object sender, RoutedEventArgs e)
        {
            var model = new KeyCeremonyDialogViewModel
            {
                IsReunification = true,
                Fragments = new ObservableCollection<string>(new string[Fragments])
            };
            var dialog = new KeyCeremonyDialog
            {
                DataContext = model
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    KeyValue = ComputeFragments(SelectedCeremonyType, model.Fragments.ToArray());
                }
                catch (Exception ex)
                {
                    log.Error("Error during the key ceremony.", ex);
                    MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSharingCeremony_Click(object sender, RoutedEventArgs e)
        {
            var model = new KeyCeremonyDialogViewModel
            {
                IsReunification = false
            };
            var fragments = CreateFragments(SelectedCeremonyType, KeyValue, Fragments);
            foreach (var fragment in fragments)
            {
                model.Fragments.Add(fragment);
            }

            var dialog = new KeyCeremonyDialog
            {
                DataContext = model
            };
            dialog.ShowDialog();
        }

        private static string[] CreateFragments(KeyCeremonyType ceremonyType, string? keyValue, int nbFragments)
        {
            throw new NotImplementedException();
        }

        private static string ComputeFragments(KeyCeremonyType ceremonyType, string[] fragments)
        {
            string keystr = string.Empty;
            switch (ceremonyType)
            {
                case KeyCeremonyType.Concat:
                    {
                        keystr = string.Join("", fragments);
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
                        var shares = string.Join(Environment.NewLine, fragments);
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
