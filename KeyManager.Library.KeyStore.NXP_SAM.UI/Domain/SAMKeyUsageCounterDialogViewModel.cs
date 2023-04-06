using Leosac.KeyManager.Library.Plugin.UI.Domain;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain
{
    public class SAMKeyUsageCounterDialogViewModel : KMObject
    {
        public SAMKeyUsageCounterDialogViewModel()
        {
            _counter = new SAMKeyUsageCounter();
        }

        private SAMKeyUsageCounter _counter;

        public SAMKeyUsageCounter Counter
        {
            get => _counter;
            set => SetProperty(ref _counter, value);
        }
    }
}
