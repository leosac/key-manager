using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.UI;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;

namespace Leosac.KeyManager.Domain
{
    public class HomeControlViewModel : ObservableValidator
    {
        public HomeControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            ElevateCommand = new RelayCommand(Elevate);
        }

        private string? _elevationCode;
        public string? ElevationCode
        {
            get => _elevationCode;
            set => SetProperty(ref _elevationCode, value);
        }

        private void Elevate()
        {
            if (!string.IsNullOrEmpty(ElevationCode))
            {
                var kmsettings = KMSettings.LoadFromFile(false);
                kmsettings?.Elevate(ElevationCode);
                DialogHost.CloseDialogCommand.Execute(null, null);

                if (UIPreferences.IsUserElevated)
                {
                    // At this moment we have to recreate all controls as some bindings are static
                    var oldWindow = Application.Current.MainWindow;
                    oldWindow.Hide();
                    var newWindow = new WpfApp.MainWindow();
                    Application.Current.MainWindow = newWindow;
                    newWindow.Show();
                    oldWindow.Close();
                }
            }
        }

        public AsyncRelayCommand<object>? KeyStoreCommand { get; set; }

        public RelayCommand? FavoritesCommand { get; set; }

        public RelayCommand? ElevateCommand { get; set; }
    }
}
