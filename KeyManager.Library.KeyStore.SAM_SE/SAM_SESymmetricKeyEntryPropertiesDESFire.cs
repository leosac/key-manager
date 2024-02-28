using CommunityToolkit.Mvvm.ComponentModel;
using System.Text;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryPropertiesDESFire : ObservableValidator
    {
        public enum SAM_SEDESFireMode : byte
        {
            Disable = 0x00,
            IDP = 0x01,
            UID = 0x02,
        }

        public enum SAM_SEDESFireEncryptMode : byte
        {
            DES = 0x00,
            AES = 0x02,
        }

        public enum SAM_SEDESFireCommunicationMode : byte
        {
            Plain = 0x00,
            Encrypted = 0x01,
        }

        public SAM_SESymmetricKeyEntryPropertiesDESFireDiv Div { get; set; } = new();

        private SAM_SEDESFireMode mode = SAM_SEDESFireMode.IDP;
        private SAM_SEDESFireEncryptMode encrypt = SAM_SEDESFireEncryptMode.AES;
        private SAM_SEDESFireCommunicationMode comm = SAM_SEDESFireCommunicationMode.Encrypted;
        private bool ev2 = true;
        private bool ev3 = true;
        private bool authEv2 = true;
        private bool authEv2Enable = true;
        private bool proximityCheckEnable = true;
        private bool proximityCheck = false;
        private bool readingModeWarning = false;
        private bool communicationWarning = false;
        private bool encryptTypeWarning = false;
        private byte size = 5;
        private byte offset = 0;
        private byte offsetMax = 94;
        private bool warning = false;
        private bool paramEnable = true;
        private bool paramTechnoEnable = true;
        private bool previousConfEnable = false;

        public bool Ev2
        {
            get { return ev2; }
            set
            {
                if (SetProperty(ref ev2, value))
                    UpdateAuthEv2Enable();
            }
        }
        public bool Ev3
        {
            get { return ev3; }
            set
            {
                if (SetProperty(ref ev3, value))
                    UpdateAuthEv2Enable();
            }
        }
        public bool AuthEv2
        {
            get { return authEv2; }
            set
            {
                if (SetProperty(ref authEv2, value))
                    UpdateProximityCheckEnable();
            }
        }
        public bool AuthEv2Enable
        {
            get { return authEv2Enable; }
            set { SetProperty(ref authEv2Enable, value); }
        }
        public bool ProximityCheck
        {
            get { return proximityCheck; }
            set { SetProperty(ref proximityCheck, value); }
        }
        public bool ProximityCheckEnable
        {
            get { return proximityCheckEnable; }
            set { SetProperty(ref proximityCheckEnable, value); }
        }
        public bool ReadingModeWarning
        {
            get { return readingModeWarning; }
            set { SetProperty(ref readingModeWarning, value); }
        }
        public SAM_SEDESFireMode ReadingMode
        {
            get { return mode; }
            set
            {
                if (value == SAM_SEDESFireMode.UID)
                {
                    value = SAM_SEDESFireMode.Disable;
                    ReadingModeWarning = true;
                }
                if (value == SAM_SEDESFireMode.IDP)
                {
                    ReadingModeWarning = false;
                }
                SetProperty(ref mode, value);
                UpdateParamEnable();
            }
        }
        public bool EncryptTypeWarning
        {
            get { return encryptTypeWarning; }
            set { SetProperty(ref encryptTypeWarning, value); }
        }
        public SAM_SEDESFireEncryptMode EncryptType
        {
            get { return encrypt; }
            set 
            {
                if (value == SAM_SEDESFireEncryptMode.DES)
                {
                    value = SAM_SEDESFireEncryptMode.AES;
                    if (ReadingMode == SAM_SEDESFireMode.IDP)
                        EncryptTypeWarning = true;
                }
                SetProperty(ref encrypt, value);
            }
        }
        public bool CommunicationWarning
        {
            get { return communicationWarning; }
            set { SetProperty(ref communicationWarning, value); }
        }
        public SAM_SEDESFireCommunicationMode Communication
        {
            get { return comm; }
            set
            {
                if (value == SAM_SEDESFireCommunicationMode.Plain)
                {
                    value = SAM_SEDESFireCommunicationMode.Encrypted;
                    if (ReadingMode == SAM_SEDESFireMode.IDP)
                        CommunicationWarning = true;
                }
                SetProperty(ref comm, value);
            }
        }
        public bool Warning
        {
            get { return warning; }
            set { SetProperty(ref warning, value); }
        }
        public bool ParamEnable
        {
            get { return paramEnable; }
            set
            {
                if (SetProperty(ref paramEnable, value))
                {
                    UpdateAuthEv2Enable();
                    UpdateProximityCheckEnable();
                    UpdateDivActive();
                }
            }
        }        
        public bool ParamTechnoEnable
        {
            get { return paramTechnoEnable; }
            set
            {
                SetProperty(ref paramTechnoEnable, value);
            }
        }
        public byte Size
        {
            get { return size; }
            set
            {
                if (SetProperty(ref size, value))
                {
                    UpdateOffsetMax();
                    UpdateWarning();
                }
            }
        }
        public byte Offset
        {
            get { return offset; }
            set { SetProperty(ref offset, value); }
        }
        public byte OffsetMax
        {
            get { return offsetMax; }
            set
            {
                if (SetProperty(ref offsetMax, value))
                    UpdateOffset();
            }
        }

        public bool PreviousConfEnable
        {
            get { return previousConfEnable; }
            set
            {
                SetProperty(ref previousConfEnable, value);
            }
        }

        public bool Msb { get; set; } = true;
        public bool Ev0 { get; set; } = true;
        public bool Ev1 { get; set; } = true;
        public bool Jcop { get; set; } = true;

        private string? aidString = "F54130";
        public string? AidString
        { 
            get => aidString;
            set
            {
                SetProperty(ref aidString, value);
                if (value == null || value.Length == 0 || value == string.Empty)
                {
                    WarningAid = true;
                    Aid = null;
                    WarningAidMessage = Properties.Resources.AidWarningMissing;
                }
                else if (value.Length != 6)
                {
                    WarningAid = true;
                    Aid = null;
                    WarningAidMessage = Properties.Resources.AidWarningWrongLength;
                }
                else
                {
                    WarningAid = false;
                    Aid = Encoding.UTF8.GetBytes(value);
                    WarningAidMessage = string.Empty;
                }
            }
        }

        private bool warningAid = false;
        public bool WarningAid 
        { 
            get => warningAid;
            set
            {
                SetProperty(ref warningAid, value);
            }
        }

        private string warningAidMessage = string.Empty;
        public string WarningAidMessage
        {
            get => warningAidMessage;
            set
            {
                SetProperty(ref warningAidMessage, value);
            }
        }

        public byte[]? Aid { get; set; } = [0xF5, 0x41, 0x30];

        public byte KeyNum { get; set; } = 1;
        public byte FileNum { get; set; } = 1;


        private void UpdateDivActive()
        {
            if(ParamEnable == false)
            {
                Div.Enable = false;
            }
        }
        private void UpdateParamEnable()
        {
            if (ReadingMode == SAM_SEDESFireMode.Disable)
                ParamTechnoEnable = false;
            else
                ParamTechnoEnable = true;
            if (ReadingMode == SAM_SEDESFireMode.IDP)
                ParamEnable = true;
            else
                ParamEnable = false;
        }
        private void UpdateAuthEv2Enable()
        {
            if ((Ev2 == true || Ev3 == true) && ParamEnable == true)
            {
                AuthEv2Enable = true;
            }
            else
            {
                AuthEv2Enable = false;
                AuthEv2 = false;
            }
        }
        private void UpdateProximityCheckEnable()
        {
            if (AuthEv2 == true && ParamEnable == true)
            {
                ProximityCheckEnable = true;
            }
            else
            {
                ProximityCheckEnable = false;
                ProximityCheck = false;
            }
        }
        private void UpdateOffsetMax()
        {
            OffsetMax = (byte)(99 - Size);
        }
        private void UpdateOffset()
        {
            if(Offset > OffsetMax)
                Offset = OffsetMax;
        }
        private void UpdateWarning()
        {
            if (Size > 5)
                Warning = true;
            else
                Warning = false;
        }
    }
}
