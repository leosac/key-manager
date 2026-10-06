using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyGenerationDialog.xaml
    /// </summary>
    public partial class KeyGenerationDialog : UserControl
    {
        public KeyGenerationDialog()
        {
            MnemonicLanguages = new ObservableCollection<KeyGen.Mnemonic.WordlistLang>(Enum.GetValues<KeyGen.Mnemonic.WordlistLang>());
            MnemonicWords = new ObservableCollection<string>();
            RandomGenerators = Favorites.GetSingletonInstance()?.KeyStores ?? new ObservableCollection<Favorite>();

            InitializeComponent();
        }

        public ObservableCollection<KeyGen.Mnemonic.WordlistLang> MnemonicLanguages { get; set; }

        public KeyGen.Mnemonic.WordlistLang SelectedMnemonicLanguage
        {
            get { return (KeyGen.Mnemonic.WordlistLang)GetValue(SelectedMnemonicLanguageProperty); }
            set { SetValue(SelectedMnemonicLanguageProperty, value); }
        }

        public static readonly DependencyProperty SelectedMnemonicLanguageProperty = DependencyProperty.Register(nameof(SelectedMnemonicLanguage), typeof(KeyGen.Mnemonic.WordlistLang), typeof(KeyGenerationDialog),
            new FrameworkPropertyMetadata(KeyGen.Mnemonic.WordlistLang.English));

        public ObservableCollection<Favorite> RandomGenerators { get; set; }

        public Favorite? SelectedRandomGenerator
        {
            get { return (Favorite?)GetValue(SelectedRandomGeneratorProperty); }
            set { SetValue(SelectedRandomGeneratorProperty, value); }
        }

        public static readonly DependencyProperty SelectedRandomGeneratorProperty = DependencyProperty.Register(nameof(SelectedRandomGenerator), typeof(Favorite), typeof(KeyGenerationDialog),
            new FrameworkPropertyMetadata(null));

        public uint KeySize
        {
            get { return (uint)GetValue(KeySizeProperty); }
            set { SetValue(KeySizeProperty, value); }
        }

        public static readonly DependencyProperty KeySizeProperty = DependencyProperty.Register(nameof(KeySize), typeof(uint), typeof(KeyGenerationDialog),
            new FrameworkPropertyMetadata((uint)128));

        public string? KeyValue
        {
            get { return Key?.GetAggregatedValueAsString() ?? (string?)GetValue(KeyValueProperty); }
            set
            {
                if (Key is Key key)
                {
                    try
                    {
                        key.SetAggregatedValueAsString(value);
                        SynchronizeKeyValue(key);
                    }
                    catch (Exception) { }
                }
                else
                {
                    SetValue(KeyValueProperty, value);
                }
            }
        }

        public static readonly DependencyProperty KeyValueProperty = DependencyProperty.Register(nameof(KeyValue), typeof(string), typeof(KeyGenerationDialog),
            new FrameworkPropertyMetadata(null, OnKeyValueChanged));

        public Key? Key
        {
            get { return (Key?)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(Key), typeof(Key), typeof(KeyGenerationDialog),
            new FrameworkPropertyMetadata(null, OnKeyChanged));

        private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Key key)
            {
                ((KeyGenerationDialog)d).SynchronizeKeyValue(key);
            }
        }

        private static void OnKeyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((KeyGenerationDialog)d).Key is Key key)
            {
                try
                {
                    key.SetAggregatedValueAsString(e.NewValue as string);
                    ((KeyGenerationDialog)d).SynchronizeKeyValue(key);
                }
                catch (Exception) { }
            }
        }

        private void SynchronizeKeyValue(Key key)
        {
            var value = string.Empty;
            try
            {
                value = key.GetAggregatedValueAsString();
            }
            catch (Exception) { }
            if (!Equals(GetValue(KeyValueProperty), value))
            {
                SetCurrentValue(KeyValueProperty, value);
            }
        }

        public ObservableCollection<string> MnemonicWords { get; set; }

        public int SelectedWordIndex
        {
            get { return (int)GetValue(SelectedWordIndexProperty); }
            set { SetValue(SelectedWordIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedWordIndexProperty = DependencyProperty.Register(nameof(SelectedWordIndex), typeof(int), typeof(KeyGenerationDialog));

        public bool KeyGenerated
        {
            get { return (bool)GetValue(KeyGeneratedProperty); }
            set { SetValue(KeyGeneratedProperty, value); }
        }

        public static readonly DependencyProperty KeyGeneratedProperty = DependencyProperty.Register(nameof(KeyGenerated), typeof(bool), typeof(KeyGenerationDialog));

        private async void BtnRandom_Click(object sender, RoutedEventArgs e)
        {
            var keySize = (byte)(KeySize > 0 ? KeySize : 128);
            byte[]? bytes = null;
            if (SelectedRandomGenerator != null)
            {
                var ks = SelectedRandomGenerator.CreateKeyStore();
                if (ks != null)
                {
                    try
                    {
                        await ks.Open();
                        bytes = await ks.GenerateBytes((byte)KeyGeneration.BitsToBytes(keySize));
                        await ks.Close(true);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                if (Key != null)
                {
                    Key.Generate(KeySize);
                    SynchronizeKeyValue(Key);
                    ShowKeyComputationConfirmation();
                }
                else
                {
                    bytes = KeyGeneration.RandomBits(keySize);
                }
            }

            if (bytes != null)
            {
                KeyValue = Convert.ToHexString(bytes);
                ShowKeyComputationConfirmation();
            }
        }

        private void BtnPassword_Click(object sender, RoutedEventArgs e)
        {
            KeyValue = Convert.ToHexString(KeyGeneration.FromPassword(tbxPassword.Password, tbxSalt.Text, KeyGeneration.BitsToBytes(KeySize)));
            ShowKeyComputationConfirmation();
        }

        private void BtnImportMnemonic_Click(object sender, RoutedEventArgs e)
        {
            var bip39 = new KeyGen.Mnemonic.BIP39();
            KeyValue = bip39.MnemonicToSeedHex(String.Join(" ", MnemonicWords), tbxMnemonicPassphrase.Password, KeyGeneration.BitsToBytes(KeySize));
            ShowKeyComputationConfirmation();
        }

        private void BtnGenerateMnemonic_Click(object sender, RoutedEventArgs e)
        {
            var bip39 = new KeyGen.Mnemonic.BIP39();
            MnemonicWords.Clear();
            var words = bip39.GenerateMnemonic(256, SelectedMnemonicLanguage).Split(' ');
            foreach(var word in words)
            {
                MnemonicWords.Add(word);
            }
        }

        private void TbxNewWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            var words = tbxNewWord.Text.Split(' ');
            if (words.Length > 1)
            {
                AddWordInput(words);
                tbxNewWord.Text = String.Empty;
            }
        }

        private void AddWordInput(params string[] words)
        {
            foreach (string word in words)
            {
                var w = word.Trim();
                if (!string.IsNullOrEmpty(w))
                {
                    MnemonicWords.Add(w);
                }
            }
        }

        private void RemoveWordClick(object sender, RoutedEventArgs e)
        {
            if (e.Source is MaterialDesignThemes.Wpf.Chip chip)
            {
                var w = chip.Content as string;
                if (!string.IsNullOrEmpty(w))
                {
                    MnemonicWords.Remove(w);
                }
            }
        }

        private void TbxNewWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                AddWordInput(tbxNewWord.Text);
                tbxNewWord.Text = string.Empty;
                e.Handled = true;
            }
        }

        private void BtnCopyMnemonic_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(string.Join(" ", MnemonicWords));
        }

        private void ShowKeyComputationConfirmation()
        {
            KeyGenerated = true;
        }
    }
}
