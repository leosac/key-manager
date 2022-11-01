using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Domain
{
    public class EditKeyStoreViewModel : ViewModelBase
    {
        public EditKeyStoreViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {

        }

        private KeyStore? _keyStore;

        public KeyStore? KeyStore
        {
            get => _keyStore;
            set => SetProperty(ref _keyStore, value);
        }
    }
}
