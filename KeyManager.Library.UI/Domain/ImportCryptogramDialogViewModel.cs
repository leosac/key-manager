using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin.Domain;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class ImportCryptogramDialogViewModel : ViewModelBase
    {
        public ImportCryptogramDialogViewModel()
        {
            _cryptogram = new KeyEntryCryptogram();
            _canChangeIdentifier = true;
        }

        private KeyEntryCryptogram _cryptogram;

        public KeyEntryCryptogram Cryptogram
        {
            get => _cryptogram;
            set => SetProperty(ref _cryptogram, value);
        }

        private bool _canChangeIdentifier;

        public bool CanChangeIdentifier
        {
            get => _canChangeIdentifier;
            set => SetProperty(ref _canChangeIdentifier, value);
        }
    }
}
