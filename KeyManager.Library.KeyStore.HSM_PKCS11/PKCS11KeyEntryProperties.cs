namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public class PKCS11KeyEntryProperties : KeyEntryProperties
    {
        public PKCS11KeyEntryProperties()
        {
            Encrypt = true;
            Decrypt = true;
            Modifiable = true;
            Sensitive = true;
        }

        public bool? Encrypt { get; set; }

        public bool? Decrypt { get; set; }

        public bool? Sign { get; set; }

        public bool? Derive { get; set; }

        public bool? Private { get; set; }

        public bool? Modifiable   { get; set; }

        public bool? Extractable { get; set; }

        public bool? Sensitive { get; set; }
    }
}
