﻿using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Domain
{
    public class HomeViewModel : ViewModelBase
    {
        public HomeViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {

        }

        public KeyManagerCommandImplementation? KeyStoreCommand { get; set; }

        public KeyManagerCommandImplementation? FavoritesCommand { get; set; }
    }
}
