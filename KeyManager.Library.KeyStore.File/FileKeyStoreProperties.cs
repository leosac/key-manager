using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Leosac.KeyManager.Library.KeyStore.File
{
    public class FileKeyStoreProperties : KeyStoreProperties, IEquatable<FileKeyStoreProperties>
    {
        public FileKeyStoreProperties() : base()
        {
            _fullpath = String.Empty;
        }

        private string _fullpath;

        public string Fullpath
        {
            get => _fullpath;
            set => SetProperty(ref _fullpath, value);
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as FileKeyStoreProperties);
        }

        public bool Equals(FileKeyStoreProperties? p)
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

        public static bool operator ==(FileKeyStoreProperties lhs, FileKeyStoreProperties rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                    return true;

                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(FileKeyStoreProperties lhs, FileKeyStoreProperties rhs) => !(lhs == rhs);
    }
}
