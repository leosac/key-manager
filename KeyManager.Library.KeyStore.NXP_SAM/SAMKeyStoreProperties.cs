using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMKeyStoreProperties : KeyStoreProperties
    {
        public SAMKeyStoreProperties() : base()
        {
            _readerProvider = "PCSC";
            _readerUnit = String.Empty;
            _autoSwitchToAV2 = true;
        }

        private string _readerProvider;

        public string ReaderProvider
        {
            get => _readerProvider;
            set => SetProperty(ref _readerProvider, value);
        }

        private string _readerUnit;

        public string ReaderUnit
        {
            get => _readerUnit;
            set => SetProperty(ref _readerUnit, value);
        }

        private bool _autoSwitchToAV2;

        public bool AutoSwitchToAV2
        {
            get => _autoSwitchToAV2;
            set => SetProperty(ref _autoSwitchToAV2, value);
        }
    }
}
