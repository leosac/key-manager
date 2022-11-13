﻿using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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