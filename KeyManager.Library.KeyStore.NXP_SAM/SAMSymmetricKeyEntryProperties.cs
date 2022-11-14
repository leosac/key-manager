using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMSymmetricKeyEntryProperties : KeyEntryProperties
    {
        public SAMKeyEntryType SAMKeyEntryType { get; set; } = SAMKeyEntryType.Host;

        public bool EnableDumpSessionKey { get; set; } = false;

        public bool CryptoBasedOnSecretKey { get; set; } = false;

        public bool KeepIV { get; set; } = false;

        public bool LockUnlock { get; set; } = false;

        public bool AuthenticateHost { get; set; } = true;

        public bool DisableChangeKeyPICC { get; set; } = false;

        public bool DisableDecryptData { get; set; } = false;

        public bool DisableEncryptData { get; set; } = false;

        public bool DisableVerifyMACFromPICC { get; set; } = false;

        public bool DisableGenerateMACFromPICC { get; set; } = false;

        public bool DisableKeyEntry { get; set; } = false;

        public bool AllowDumpSecretKey { get; set; } = false;

        public bool AllowDumpSecretKeyWithDiv { get; set; } = false;

        public byte[] DESFireAID { get; set; } = new byte[3];

        public byte DESFireKeyNum { get; set; } = 0;

        public byte? KeyUsageCounter { get; set; } = null;

        public byte ChangeKeyRefId { get; set; } = 0;

        public byte ChangeKeyRefVersion { get; set; } = 0;
    }
}
