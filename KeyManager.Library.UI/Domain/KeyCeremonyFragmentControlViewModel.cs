using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
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
                var control = new KeyPrintControl
                {
                    Key = new Key(new[] { "FRAGMENT" }, FragmentValue)
                };

                printDialog.PrintVisual(control, "Leosac Key Manager - Key Fragment Printing");
            }
        }
    }
}
