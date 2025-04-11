namespace Leosac.KeyManager.Library.KeyStore.CNG
{
    public class CNGKeyEntryProperties : KeyEntryProperties
    {
        private bool _usageAllowAll = true;
        public bool UsageAllowAll
        {
            get => _usageAllowAll;
            set => SetProperty(ref _usageAllowAll, value);
        }

        private bool _usageAllowDecrypt;
        public bool UsageAllowDecrypt
        {
            get => _usageAllowDecrypt;
            set => SetProperty(ref _usageAllowDecrypt, value);
        }

        private bool _usageAllowSigning;
        public bool UsageAllowSigning
        {
            get => _usageAllowSigning;
            set => SetProperty(ref _usageAllowSigning, value);
        }

        private bool _usageAllowKeyAgreement;
        public bool UsageAllowKeyAgreement
        {
            get => _usageAllowKeyAgreement;
            set => SetProperty(ref _usageAllowKeyAgreement, value);
        }

        private bool _usageAllowKeyAttestation;
        public bool UsageAllowKeyAttestation
        {
            get => _usageAllowKeyAttestation;
            set => SetProperty(ref _usageAllowKeyAttestation, value);
        }

        private bool _exportAllowExport;
        public bool ExportAllowExport
        {
            get => _exportAllowExport;
            set => SetProperty(ref _exportAllowExport, value);
        }

        private bool _exportAllowPlainExport;
        public bool ExportAllowPlainExport
        {
            get => _exportAllowPlainExport;
            set => SetProperty(ref _exportAllowPlainExport, value);
        }

        private bool _exportAllowArchiving;
        public bool ExportAllowArchiving
        {
            get => _exportAllowArchiving;
            set => SetProperty(ref _exportAllowArchiving, value);
        }

        private bool _exportAllowPlainArchiving;
        public bool ExportAllowPlainArchiving
        {
            get => _exportAllowPlainArchiving;
            set => SetProperty(ref _exportAllowPlainArchiving, value);
        }
    }
}
