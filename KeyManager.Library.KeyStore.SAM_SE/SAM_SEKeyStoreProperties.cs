/*
** File Name: SAM_SEKeyStoreProperties.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file store properties of a SAM-SE Key Store.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.Properties;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SEKeyStoreProperties : KeyStoreProperties, IEquatable<SAM_SEKeyStoreProperties>
    {
        public SAM_SEKeyStoreProperties() : base()
        {
            Secret = string.Empty;
        }

        //Dictionnary to link a lock level value to a string in FR/EN
        public static Dictionary<SAM_SELockLevel, string> SAM_SELockLvl { get; } =
            new Dictionary<SAM_SELockLevel, string>()
            {
                {SAM_SELockLevel.LOCK_LVL_KEYS, Resources.LockKey},
                {SAM_SELockLevel.LOCK_LVL_FILE, Resources.LockDefault},
                {SAM_SELockLevel.LOCK_LVL_READONLY, Resources.LockReadOnly},
            };

        private string lockedLevelString = Resources.LockDefault;
        public string LockedLevelString
        { 
            get => lockedLevelString;
            set { SetProperty(ref lockedLevelString, value); }
        }

        public string ProgrammingStationPath {  get; set; } = string.Empty;

        private SAM_SELockLevel lockedLevel = SAM_SELockLevel.LOCK_LVL_FILE;
        public SAM_SELockLevel LockedLevel
        { 
            get => lockedLevel;
            set
            {
                if (SetProperty(ref lockedLevel, value))
                {
                    LockedLevelString = SAM_SELockLvl[value];
                    Locked = (uint)value;
                }
            }
        }

        private uint locked = (uint)SAM_SELockLevel.LOCK_LVL_FILE;
        public uint Locked
        {
            get => locked;
            set
            {
                if (SetProperty(ref locked, value))
                    LockedLevel = (SAM_SELockLevel)value;
            }
        }

        private bool defaultKey = true;
        public bool DefaultKey
        {
            get => defaultKey;
            set
            {
                if (SetProperty(ref defaultKey, value))
                {
                    Secret = string.Empty;
                }
            }
        }

        private bool autoUpdate = false;
        public bool AutoUpdate
        {
            get => autoUpdate;
            set => SetProperty(ref autoUpdate, value);
        }

        private bool autoLock = false;
        public bool AutoLock
        {
            get => autoLock;
            set => SetProperty(ref autoLock, value);
        }

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

            return (ProgrammingStationPath == p.ProgrammingStationPath);
        }

        public override int GetHashCode() => (ProgrammingStationPath).GetHashCode();

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
