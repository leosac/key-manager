using Leosac.WpfApp;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for SymmetricKeyGenerationDialog.xaml
    /// </summary>
    public partial class SymmetricKeyGenerationDialog : UserControl
    {
        public SymmetricKeyGenerationDialog()
        {
            MnemonicLanguages = new ObservableCollection<Mnemonic.WordlistLang>(Enum.GetValues<Mnemonic.WordlistLang>());
            MnemonicWords = new ObservableCollection<string>();

            InitializeComponent();
        }

        public ObservableCollection<Mnemonic.WordlistLang> MnemonicLanguages { get; set; }

        public Mnemonic.WordlistLang SelectedMnemonicLanguage
        {
            get { return (Mnemonic.WordlistLang)GetValue(SelectedMnemonicLanguageProperty); }
            set { SetValue(SelectedMnemonicLanguageProperty, value); }
        }

        public static readonly DependencyProperty SelectedMnemonicLanguageProperty = DependencyProperty.Register(nameof(SelectedMnemonicLanguage), typeof(Mnemonic.WordlistLang), typeof(SymmetricKeyGenerationDialog),
            new FrameworkPropertyMetadata(Mnemonic.WordlistLang.English));

        public int KeySize
        {
            get { return (int)GetValue(KeySizeProperty); }
            set { SetValue(KeySizeProperty, value); }
        }

        public static readonly DependencyProperty KeySizeProperty = DependencyProperty.Register(nameof(KeySize), typeof(int), typeof(SymmetricKeyGenerationDialog),
            new FrameworkPropertyMetadata(16));

        public string? KeyValue
        {
            get { return (string)GetValue(KeyValueProperty); }
            set { SetValue(KeyValueProperty, value); }
        }

        public static readonly DependencyProperty KeyValueProperty = DependencyProperty.Register(nameof(KeyValue), typeof(string), typeof(SymmetricKeyGenerationDialog));

        public ObservableCollection<string> MnemonicWords { get; set; }

        public int SelectedWordIndex
        {
            get { return (int)GetValue(SelectedWordIndexProperty); }
            set { SetValue(SelectedWordIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedWordIndexProperty = DependencyProperty.Register(nameof(SelectedWordIndex), typeof(int), typeof(SymmetricKeyGenerationDialog));

        private void BtnRandom_Click(object sender, RoutedEventArgs e)
        {
            KeyValue = Convert.ToHexString(KeyGeneration.Random((uint)(KeySize > 0 ? KeySize : 16)));
            ShowKeyComputationConfirmation();
        }

        private void BtnPassword_Click(object sender, RoutedEventArgs e)
        {
            KeyValue = Convert.ToHexString(KeyGeneration.FromPassword(tbxPassword.Password, tbxSalt.Text, KeySize));
            ShowKeyComputationConfirmation();
        }

        private void BtnImportMnemonic_Click(object sender, RoutedEventArgs e)
        {
            var bip39 = new Mnemonic.BIP39();
            KeyValue = bip39.MnemonicToSeedHex(String.Join(" ", MnemonicWords), tbxMnemonicPassphrase.Password, KeySize);
            ShowKeyComputationConfirmation();
        }

        private void BtnGenerateMnemonic_Click(object sender, RoutedEventArgs e)
        {
            var bip39 = new Mnemonic.BIP39();
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
            SnackbarHelper.EnqueueMessage(keyGenSnackbar.MessageQueue, Properties.Resources.KeyComputationConfirmation);
        }
    }
}
