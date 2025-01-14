/*
** File Name: SAM_SESymmetricKeyEntryProperties.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file regroups all properties of a generic SAM-SE Key Entry.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryProperties : KeyEntryProperties
    {
        public SAM_SESymmetricKeyEntryPoliciesProperties Politics { get; set; } = new();

        //Enum listing the different types of keys. This enum is from SPSE_DLL
        //Do not modify this enum without any knowledge of the SPSE_DLL
        public enum SAM_SEKeyEntryType : byte
        {
            Default = 0x00,
            Authenticate,
            DESFire,
            DESFireUID,
        }

        private SAM_SEKeyEntryType keyEntryType = SAM_SEKeyEntryType.Default;
        public SAM_SEKeyEntryType KeyEntryType
        {
            get => keyEntryType;
            set
            {
                SetProperty(ref keyEntryType, value);
            }
        }
    }
}
