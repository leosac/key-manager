using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Domain
{
    public class FavoritesControlViewModel : ViewModelBase
    {
        public FavoritesControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _favorites = Favorites.GetSingletonInstance();
        }

        private Favorites? _favorites;

        public Favorites? Favorites
        {
            get => _favorites;
            set => SetProperty(ref _favorites, value);
        }

        public KeyManagerCommand? KeyStoreCommand { get; set; }
    }
}
