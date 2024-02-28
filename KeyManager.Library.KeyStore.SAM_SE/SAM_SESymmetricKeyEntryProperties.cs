namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryProperties : KeyEntryProperties
    {
        //Enum listing the different types of keys. This enum is from SPSE_DLL
        //Do not modify this enum without any knowledge of the SPSE_DLL
        public enum SAM_SEKeyEntryType : byte
        {
            Default = 0x00,
            Authenticate,
            DESFire,
            DESFireUID,
        }

        public SAM_SESymmetricKeyEntryPropertiesAuthenticate Authenticate { get; set; } = new();
        public SAM_SESymmetricKeyEntryPropertiesDESFireUID DESFireUID { get; set; } = new();
        public SAM_SESymmetricKeyEntryPropertiesDESFire DESFire { get; set; } = new();
        public SAM_SESymmetricKeyEntryPropertiesPolitics Politics { get; set; } = new();
        public byte? KeyUsageCounter { get; set; } = null;
        public byte ChangeKeyRefId { get; set; } = 0;
        public byte ChangeKeyRefVersion { get; set; } = 0;    
        public bool KeyEntryPasswordShow { get; set; } = false;
        public bool KeyEntryDESFireShow { get; set; } = false;
        public bool KeyEntryDESFireUidShow { get; set; } = false;

        private SAM_SEKeyEntryType keyEntryType = SAM_SEKeyEntryType.Default;
        public SAM_SEKeyEntryType KeyEntryType
        {
            get => keyEntryType;
            set
            {
                SetProperty(ref keyEntryType, value);
                UpdateDisplayOptions();
            }
        }

        private void UpdateDisplayOptions()
        {
            KeyEntryPasswordShow = false;
            KeyEntryDESFireShow = false;
            KeyEntryDESFireUidShow = false;
            switch (KeyEntryType)
            {
                case SAM_SEKeyEntryType.DESFire:
                    KeyEntryDESFireShow = true;
                    break;                
                case SAM_SEKeyEntryType.DESFireUID:
                    KeyEntryDESFireUidShow = true;
                    break;                
                case SAM_SEKeyEntryType.Authenticate:
                    KeyEntryPasswordShow = true;
                    break;
                default:
                    break;
            }
        }
    }
}
