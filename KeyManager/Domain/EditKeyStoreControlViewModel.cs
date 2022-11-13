using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.KeyStore;
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
    public class EditKeyStoreControlViewModel : KeyStoreControlViewModel
    {
        public EditKeyStoreControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
            : base(snackbarMessageQueue)
        {

        }

        private Favorite? _favorite;

        public Favorite? Favorite
        {
            get => _favorite;
            set => SetProperty(ref _favorite, value);
        }

        public KeyManagerCommand? HomeCommand { get; set; }
    }
}
