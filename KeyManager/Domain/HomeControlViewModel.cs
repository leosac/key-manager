using Leosac.KeyManager.Library.Plugin.Domain;
using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;

namespace Leosac.KeyManager.Domain
{
    public class HomeControlViewModel : ViewModelBase
    {
        public HomeControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {

        }

        public KeyManagerCommand? KeyStoreCommand { get; set; }

        public KeyManagerCommand? FavoritesCommand { get; set; }
    }
}
