using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace Leosac.KeyManager.Domain
{
    public class HomeControlViewModel : ObservableValidator
    {
        public HomeControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {

        }

        public AsyncRelayCommand<object>? KeyStoreCommand { get; set; }

        public RelayCommand? FavoritesCommand { get; set; }
    }
}
