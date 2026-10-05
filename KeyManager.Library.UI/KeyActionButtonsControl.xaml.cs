using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.KeyManager.Library.UI.Helpers;
using log4net;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyActionButtonsControl.xaml
    /// </summary>
    public partial class KeyActionButtonsControl : UserControl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

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

        public bool ShowKeyGen
        {
            get { return (bool)GetValue(ShowKeyGenProperty); }
            set { SetValue(ShowKeyGenProperty, value); }
        }

        public static readonly DependencyProperty ShowKeyGenProperty = DependencyProperty.Register(nameof(ShowKeyGen), typeof(bool), typeof(KeyActionButtonsControl),
            new FrameworkPropertyMetadata(true));

        public bool ShowMenu
        {
            get { return (bool)GetValue(ShowMenuProperty); }
            set { SetValue(ShowMenuProperty, value); }
        }

        public static readonly DependencyProperty ShowMenuProperty = DependencyProperty.Register(nameof(ShowMenu), typeof(bool), typeof(KeyActionButtonsControl),
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

                var ret = await DialogHelper.ForceShow(dialog, "KeyEntryDialog");
                if (ret != null && !string.IsNullOrEmpty(model.LinkResult))
                {
                    Key.SetAggregatedValueAsString(model.LinkResult);
                }
            }
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            KeyFileDialogHelper.Import(
                key => Key.SetAggregatedValueAsString(Convert.ToHexString(key)),
                (value, format) => Key.SetAggregatedValueAsString(value, format ?? KeyValueStringFormat.HexString),
                (privateKey, publicKey) => Key.SetAggregatedValueAsString(
                    string.Join(Environment.NewLine, new[] { privateKey, publicKey }.Where(value => !string.IsNullOrEmpty(value))),
                    KeyValueStringFormat.Pem));
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            KeyFileDialogHelper.Export(
                () => Convert.FromHexString(Key.GetAggregatedValueAsString() ?? ""),
                format => Key.GetAggregatedValueAsString(format ?? KeyValueStringFormat.HexString) ?? string.Empty);
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
                try
                {
                    printDialog.PrintVisual(control, "Leosac Key Manager - Key Printing");
                }
                catch(Exception ex)
                {
                    log.Error("Key printing error.", ex);
                }
            }
        }

        private async void BtnQrCode_Click(object sender, RoutedEventArgs e)
        {
            var qrCode = new QrCodeControl();
            qrCode.GenerateQrCode(Key.GetAggregatedValueAsString());
            await DialogHelper.ForceShow(qrCode, "KeyEntryDialog");
        }

        private void BtnSpeech_Click(object sender, RoutedEventArgs e)
        {
            var key = Key.GetAggregatedValueAsString();
            Task.Run(() =>
            {
                var promptBuilder = new PromptBuilder();
                var promptStyle = new PromptStyle
                {
                    Volume = PromptVolume.Default,
                    Rate = PromptRate.ExtraSlow
                };
                promptBuilder.StartStyle(promptStyle);
                promptBuilder.AppendTextWithHint(key, SayAs.SpellOut);
                promptBuilder.EndStyle();

                using var synthesizer = new SpeechSynthesizer();
                synthesizer.SetOutputToDefaultAudioDevice();
                synthesizer.Speak(promptBuilder);
            });
        }
    }
}
