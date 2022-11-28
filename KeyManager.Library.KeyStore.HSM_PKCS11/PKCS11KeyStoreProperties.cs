using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public class PKCS11KeyStoreProperties : KeyStoreProperties, IEquatable<PKCS11KeyStoreProperties>
    {
        public PKCS11KeyStoreProperties()
        {
            _libraryPath = string.Empty;
            _slotFilterType = SlotFilterType.SlotId;
            _user = Net.Pkcs11Interop.Common.CKU.CKU_SO;
        }

        private string _libraryPath;

        public string LibraryPath
        {
            get => _libraryPath;
            set => SetProperty(ref _libraryPath, value);
        }

        private SlotFilterType _slotFilterType;

        public SlotFilterType SlotFilterType
        {
            get => _slotFilterType;
            set => SetProperty(ref _slotFilterType, value);
        }

        private string? _slotFilter;

        public string? SlotFilter
        {
            get => _slotFilter;
            set => SetProperty(ref _slotFilter, value);
        }

        private Net.Pkcs11Interop.Common.CKU _user;

        public Net.Pkcs11Interop.Common.CKU User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        private string? _userPIN;

        public string? UserPIN
        {
            get => _userPIN;
            set => SetProperty(ref _userPIN, value);
        }

        public byte[]? GetUserPINBytes()
        {
            if (UserPIN == null)
                return null;

            return UTF8Encoding.UTF8.GetBytes(UserPIN);
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as PKCS11KeyStoreProperties);
        }

        public bool Equals(PKCS11KeyStoreProperties? p)
        {
            if (p is null)
                return false;

            if (Object.ReferenceEquals(this, p))
                return true;

            if (this.GetType() != p.GetType())
                return false;

            return (LibraryPath == p.LibraryPath && SlotFilterType == p.SlotFilterType && SlotFilter == p.SlotFilter && User == p.User && UserPIN == p.UserPIN);
        }

        public override int GetHashCode() => (LibraryPath, SlotFilterType, SlotFilter, User, UserPIN).GetHashCode();

        public static bool operator ==(PKCS11KeyStoreProperties lhs, PKCS11KeyStoreProperties rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                    return true;

                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(PKCS11KeyStoreProperties lhs, PKCS11KeyStoreProperties rhs) => !(lhs == rhs);
    }
}
