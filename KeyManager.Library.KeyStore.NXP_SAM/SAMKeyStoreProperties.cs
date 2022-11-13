using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMKeyStoreProperties : KeyStoreProperties, IEquatable<SAMKeyStoreProperties>
    {
        public SAMKeyStoreProperties() : base()
        {
            _readerProvider = "PCSC";
            _readerUnit = String.Empty;
            _autoSwitchToAV2 = true;
        }

        private string _readerProvider;

        public string ReaderProvider
        {
            get => _readerProvider;
            set => SetProperty(ref _readerProvider, value);
        }

        private string _readerUnit;

        public string ReaderUnit
        {
            get => _readerUnit;
            set => SetProperty(ref _readerUnit, value);
        }

        private bool _autoSwitchToAV2;

        public bool AutoSwitchToAV2
        {
            get => _autoSwitchToAV2;
            set => SetProperty(ref _autoSwitchToAV2, value);
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as SAMKeyStoreProperties);
        }

        public bool Equals(SAMKeyStoreProperties? p)
        {
            if (p is null)
                return false;

            if (Object.ReferenceEquals(this, p))
                return true;

            if (this.GetType() != p.GetType())
                return false;

            return (ReaderProvider == p.ReaderProvider) && (ReaderUnit == p.ReaderUnit) && (AutoSwitchToAV2 == p.AutoSwitchToAV2);
        }

        public override int GetHashCode() => (ReaderProvider, ReaderUnit, AutoSwitchToAV2).GetHashCode();

        public static bool operator ==(SAMKeyStoreProperties lhs, SAMKeyStoreProperties rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                    return true;

                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(SAMKeyStoreProperties lhs, SAMKeyStoreProperties rhs) => !(lhs == rhs);
    }
}
