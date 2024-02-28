using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryPropertiesDESFireUID : ObservableValidator
    {
        private bool enable;
        public byte KeyNum { get; set; } = 0;
        public bool Enable
        {
            get
            {
                return enable;
            }
            set
            {
                SetProperty(ref enable, value);
            }
        }
    }
}
