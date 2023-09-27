using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SEKeyStoreProperties : KeyStoreProperties, IEquatable<SAM_SEKeyStoreProperties>
    {
        public SAM_SEKeyStoreProperties() : base()
        {
            _fullpath = String.Empty;
        }

        private string _fullpath;

        public string Fullpath
        {
            get => _fullpath;
            set => SetProperty(ref _fullpath, value);
        }

        public override int? SecretMaxLength => 32;

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as SAM_SEKeyStoreProperties);
        }

        public bool Equals(SAM_SEKeyStoreProperties? p)
        {
            if (p is null)
                return false;

            if (Object.ReferenceEquals(this, p))
                return true;

            if (this.GetType() != p.GetType())
                return false;

            return (Fullpath == p.Fullpath);
        }

        public override int GetHashCode() => (Fullpath).GetHashCode();

        public static bool operator ==(SAM_SEKeyStoreProperties? lhs, SAM_SEKeyStoreProperties? rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                    return true;

                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(SAM_SEKeyStoreProperties? lhs, SAM_SEKeyStoreProperties? rhs) => !(lhs == rhs);
    }
}
