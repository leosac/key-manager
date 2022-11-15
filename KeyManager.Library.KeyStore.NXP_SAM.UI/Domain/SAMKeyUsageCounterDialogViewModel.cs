using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain
{
    public class SAMKeyUsageCounterDialogViewModel : ViewModelBase
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
