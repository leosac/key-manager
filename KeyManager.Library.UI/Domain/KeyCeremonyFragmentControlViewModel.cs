using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Net.Codecrete.QrCodeGenerator;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyCeremonyFragmentControlViewModel : ObservableValidator
    {
        public KeyCeremonyFragmentControlViewModel()
        {
            _fragmentValue = string.Empty;
            BrowseCommand = new RelayCommand(Browse);
            PrintCommand = new RelayCommand(Print);
            GenerateQrCodeCommand = new RelayCommand(() => {
                GenerateQrCode();
                MaterialDesignThemes.Wpf.Flipper.FlipCommand.Execute(null, null);
            });
        }

        private int _fragment;
        public int Fragment
        {
            get => _fragment;
            set => SetProperty(ref _fragment, value);
        }

        private string _fragmentValue;
        public string FragmentValue
        {
            get => _fragmentValue;
            set => SetProperty(ref _fragmentValue, value);
        }

        private bool _isReunification;
        public bool IsReunification
        {
            get => _isReunification;
            set => SetProperty(ref _isReunification, value);
        }

        private byte[]? _qrCode;
        public byte[]? QrCode
        {
            get => _qrCode;
            set => SetProperty(ref _qrCode, value);
        }

        public RelayCommand GenerateQrCodeCommand { get; }

        public void GenerateQrCode()
        {
            var qr = Net.Codecrete.QrCodeGenerator.QrCode.EncodeText(FragmentValue, Net.Codecrete.QrCodeGenerator.QrCode.Ecc.Medium);
            QrCode = qr.ToPng(10, 4);
        }

        public RelayCommand BrowseCommand { get; }

        public void Browse()
        {
            if (IsReunification)
            {
                var ofd = new OpenFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt",
                    CheckFileExists = true
                };
                if (ofd.ShowDialog() == true)
                {
                    FragmentValue = System.IO.File.ReadAllText(ofd.FileName);
                }
            }
            else
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt"
                };
                if (sfd.ShowDialog() == true)
                {
                    System.IO.File.WriteAllText(sfd.FileName, FragmentValue);
                }
            }
        }

        public RelayCommand PrintCommand { get; }

        public void Print()
        {
            var printDialog = new PrintDialog();
            if (!string.IsNullOrEmpty(FragmentValue) && printDialog.ShowDialog() == true)
            {
                var control = new FragmentPrintControl
                {
                    Fragment = FragmentValue
                };

                printDialog.PrintVisual(control, "Leosac Key Manager - Key Fragment Printing");
            }
        }
    }
}
