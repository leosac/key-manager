namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMSymmetricKeyEntryProperties : KeyEntryProperties
    {
        public SAMKeyEntryType SAMKeyEntryType { get; set; } = SAMKeyEntryType.Host;

        public bool EnableDumpSessionKey { get; set; }

        public bool CryptoBasedOnSecretKey { get; set; }

        public bool KeepIV { get; set; }

        public bool LockUnlock { get; set; }

        public bool AuthenticateHost { get; set; } = true;

        public bool DisableChangeKeyPICC { get; set; }

        public bool DisableDecryptData { get; set; }

        public bool DisableEncryptData { get; set; }

        public bool DisableVerifyMACFromPICC { get; set; }

        public bool DisableGenerateMACFromPICC { get; set; }

        public bool DisableKeyEntry { get; set; }

        public bool AllowDumpSecretKey { get; set; }

        public bool AllowDumpSecretKeyWithDiv { get; set; }

        public byte[] DESFireAID { get; set; } = new byte[3];

        public byte DESFireKeyNum { get; set; }

        public byte? KeyUsageCounter { get; set; } = null;

        public byte ChangeKeyRefId { get; set; }

        public byte ChangeKeyRefVersion { get; set; }
    }
}
