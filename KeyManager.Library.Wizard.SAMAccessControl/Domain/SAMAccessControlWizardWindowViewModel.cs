using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.Wizard.SAMAccessControl.Domain
{
    public class SAMAccessControlWizardWindowViewModel : ViewModelBase
    {
        public SAMAccessControlWizardWindowViewModel()
        {
            _changeSAMMasterKey = false;
            _changeSAMUnlockKey = true;
            _piccKey = new KeyVersion("DESFire Key", 1, new Key(new string[] { "AES", KeyEntryClass.Symmetric.ToString() }, 16));
            _piccAID = new byte[3];
        }

        private bool _changeSAMMasterKey;
        private bool _changeSAMUnlockKey;
        private KeyVersion _piccKey;
        private byte[] _piccAID;
        private byte _piccKeyNo;

        public bool ChangeSAMMasterKey
        {
            get => _changeSAMMasterKey;
            set => SetProperty(ref _changeSAMMasterKey, value);
        }

        public bool ChangeSAMUnlockKey
        {
            get => _changeSAMUnlockKey;
            set => SetProperty(ref _changeSAMUnlockKey, value);
        }

        public KeyVersion PICCKey
        {
            get => _piccKey;
            set => SetProperty(ref _piccKey, value);
        }

        public byte[] PICCAID
        {
            get => _piccAID;
            set => SetProperty(ref _piccAID, value);
        }

        public byte PICCKeyNo
        {
            get => _piccKeyNo;
            set => SetProperty(ref _piccKeyNo, value);
        }
    }
}
