using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp.Domain;
using MaterialDesignThemes.Wpf;

namespace Leosac.KeyManager.Domain
{
    public class HomeControlViewModel : KMObject
    {
        public HomeControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {

        }

        public LeosacAppAsyncCommand<object>? KeyStoreCommand { get; set; }

        public LeosacAppCommand? FavoritesCommand { get; set; }
    }
}
