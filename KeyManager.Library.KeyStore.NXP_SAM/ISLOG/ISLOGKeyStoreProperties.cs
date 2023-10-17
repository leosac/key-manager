namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.ISLOG
{
    public class ISLOGKeyStoreProperties : KeyStoreProperties, IEquatable<ISLOGKeyStoreProperties>
    {
        public ISLOGKeyStoreProperties()
        {
            _templateFile = string.Empty;
        }

        private string _templateFile;

        public string TemplateFile
        {
            get => _templateFile;
            set => SetProperty(ref _templateFile, value);
        }

        public override int? SecretMaxLength => 32;

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as ISLOGKeyStoreProperties);
        }

        public bool Equals(ISLOGKeyStoreProperties? p)
        {
            if (p is null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            if (this.GetType() != p.GetType())
            {
                return false;
            }

            return (TemplateFile == p.TemplateFile);
        }

        public override int GetHashCode() => (TemplateFile).GetHashCode();

        public static bool operator ==(ISLOGKeyStoreProperties? lhs, ISLOGKeyStoreProperties? rhs)
        {
            if (lhs is null)
            {
                return rhs is null;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(ISLOGKeyStoreProperties? lhs, ISLOGKeyStoreProperties? rhs) => !(lhs == rhs);
    }
}
