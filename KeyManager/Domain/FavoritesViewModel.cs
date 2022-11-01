using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Domain
{
    public class FavoritesViewModel : ViewModelBase
    {
        public FavoritesViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _favorites = new Favorites(); //Favorites.LoadFromFile();
        }

        private Favorites _favorites;

        public Favorites Favorites
        {
            get => _favorites;
            set => SetProperty(ref _favorites, value);
        }
    }
}
