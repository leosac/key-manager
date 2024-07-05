using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using log4net;
using Microsoft.Win32;
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

                await DialogHelper.ForceShow(dialog, "KeyEntryDialog");
            }
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "Binary Files (*.bin)|*.bin|Text Files (*.txt)|*.txt"
            };
            if (ofd.ShowDialog() == true)
            {
                if (ofd.FilterIndex == 1)
                {
                    var key = System.IO.File.ReadAllBytes(ofd.FileName);
                    Key.SetAggregatedValueAsString(Convert.ToHexString(key));
                }
                else
                {
                    Key.SetAggregatedValueAsString(System.IO.File.ReadAllText(ofd.FileName));
                }
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Binary Files (*.bin)|*.bin|Text Files (*.txt)|*.txt"
            };
            if (sfd.ShowDialog() == true)
            {
                if (sfd.FilterIndex == 1)
                {
                    System.IO.File.WriteAllBytes(sfd.FileName, Convert.FromHexString(Key.GetAggregatedValueAsString() ?? ""));
                }
                else
                {
                    System.IO.File.WriteAllText(sfd.FileName, Key.GetAggregatedValueAsString() ?? "");
                }
            }
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
                printDialog.PrintVisual(control, "Leosac Key Manager - Key Printing");
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
