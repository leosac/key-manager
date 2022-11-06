using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using SecretSharingDotNet.Cryptography;
using SecretSharingDotNet.Math;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyGenerationDialog.xaml
    /// </summary>
    public partial class KeyGenerationDialog : UserControl
    {
        public KeyGenerationDialog()
        {
            CeremonyTypes = new ObservableCollection<KeyCeremonyType>(Enum.GetValues<KeyCeremonyType>());
            MnemonicLanguages = new ObservableCollection<Mnemonic.WordlistLang>(Enum.GetValues<Mnemonic.WordlistLang>());

            InitializeComponent();
        }

        public ObservableCollection<KeyCeremonyType> CeremonyTypes { get; set; }

        public KeyCeremonyType SelectedCeremonyType
        {
            get { return (KeyCeremonyType)GetValue(SelectedCeremonyTypeProperty); }
            set { SetValue(SelectedCeremonyTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedCeremonyTypeProperty = DependencyProperty.Register(nameof(SelectedCeremonyType), typeof(KeyCeremonyType), typeof(KeyGenerationDialog),
            new FrameworkPropertyMetadata(KeyCeremonyType.ShamirSecretSharing));

        public int Fragments
        {
            get { return (int)GetValue(FragmentsProperty); }
            set { SetValue(FragmentsProperty, value); }
        }

        public static readonly DependencyProperty FragmentsProperty = DependencyProperty.Register(nameof(Fragments), typeof(int), typeof(KeyGenerationDialog),
            new FrameworkPropertyMetadata(3));

        public ObservableCollection<Mnemonic.WordlistLang> MnemonicLanguages { get; set; }

        public Mnemonic.WordlistLang SelectedMnemonicLanguage
        {
            get { return (Mnemonic.WordlistLang)GetValue(SelectedMnemonicLanguageProperty); }
            set { SetValue(SelectedMnemonicLanguageProperty, value); }
        }

        public static readonly DependencyProperty SelectedMnemonicLanguageProperty = DependencyProperty.Register(nameof(SelectedMnemonicLanguage), typeof(Mnemonic.WordlistLang), typeof(KeyGenerationDialog),
            new FrameworkPropertyMetadata(Mnemonic.WordlistLang.English));

        public int KeySize
        {
            get { return (int)GetValue(KeySizeProperty); }
            set { SetValue(KeySizeProperty, value); }
        }

        public static readonly DependencyProperty KeySizeProperty = DependencyProperty.Register(nameof(KeySize), typeof(int), typeof(KeyGenerationDialog),
            new FrameworkPropertyMetadata(16));

        public string? KeyValue
        {
            get { return (string)GetValue(KeyValueProperty); }
            set { SetValue(KeyValueProperty, value); }
        }

        public static readonly DependencyProperty KeyValueProperty = DependencyProperty.Register(nameof(KeyValue), typeof(string), typeof(KeyGenerationDialog));

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var key = new byte[KeySize];
                rng.GetBytes(key);
                KeyValue = Convert.ToHexString(key);
            }
        }

        private void btnPassword_Click(object sender, RoutedEventArgs e)
        {
            var deriv = new Rfc2898DeriveBytes(tbxPassword.Password, Encoding.UTF8.GetBytes(tbxSalt.Text));
            var key = deriv.GetBytes(KeySize);
            KeyValue = Convert.ToHexString(key);
        }

        public static byte[] CreateRandomSalt(int length)
        {
            byte[] randBytes = (length >= 1) ? new byte[length] : new byte[1];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randBytes);
            }
            return randBytes;
        }

        private void btnCeremony_Click(object sender, RoutedEventArgs e)
        {
            var model = new KeyCeremonyDialogViewModel()
            {
                Fragments = new ObservableCollection<string>(new string[Fragments])
            };
            var dialog = new KeyCeremonyDialog()
            {
                DataContext = model
            };
            if (dialog.ShowDialog() == true)
            {
                KeyValue = ComputeKeyCeremony(SelectedCeremonyType, model.Fragments.ToArray());
            }
        }

        private string ComputeKeyCeremony(KeyCeremonyType ceremonyType, string[] fragments)
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
                        var key = new byte[KeySize];
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
                    break;

                case KeyCeremonyType.ShamirSecretSharing:
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

        private void btnImportMnemonic_Click(object sender, RoutedEventArgs e)
        {
            var bip39 = new Mnemonic.BIP39();
            KeyValue = bip39.MnemonicToSeedHex(tbxMnemonicWords.Text, tbxMnemonicPassphrase.Password, KeySize);
        }

        private void btnGenerateMnemonic_Click(object sender, RoutedEventArgs e)
        {
            var bip39 = new Mnemonic.BIP39();
            tbxMnemonicWords.Text = bip39.GenerateMnemonic(256, SelectedMnemonicLanguage);
        }
    }
}
