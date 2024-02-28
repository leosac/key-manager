using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryPropertiesPolitics : ObservableValidator
    {
        private bool read = false;
        public bool Read
        { 
            get => read; 
            set { SetProperty(ref read, value); } 
        }
        private bool write = false;
        public bool Write
        { 
            get => write; 
            set { SetProperty(ref write, value); } 
        }
        private bool import = false;
        public bool Import
        { 
            get => import; 
            set { SetProperty(ref import, value); } 
        }
        private bool wrap = false;
        public bool Wrap
        { 
            get => wrap; 
            set { SetProperty(ref wrap, value); } 
        }
        private bool encrypt = false;
        public bool Encrypt
        { 
            get => encrypt; 
            set { SetProperty(ref encrypt, value); } 
        }
        private bool decrypt = false;
        public bool Decrypt
        { 
            get => decrypt; 
            set { SetProperty(ref decrypt, value); } 
        }
        private bool authDESFire = false;
        public bool AuthDESFire
        { 
            get => authDESFire; 
            set { SetProperty(ref authDESFire, value); } 
        }
        private bool sessionKeyDESFire = false;
        public bool SessionKeyDESFire
        {
            get => sessionKeyDESFire;
            set { SetProperty(ref sessionKeyDESFire, value); }
        }
    }
}
