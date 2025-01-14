/*
** File Name: SAM_SESymmetricKeyEntryDESFireUIDProperties.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file regroups all properties of a SAM-SE DESFire UID Key Entry.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryDESFireUIDProperties : SAM_SESymmetricKeyEntryProperties
    {
        public SAM_SESymmetricKeyEntryDESFireUIDProperties() : base()
        {
            KeyEntryType = SAM_SEKeyEntryType.DESFireUID;
        }
        public byte KeyNum { get; set; } = 7;

        private bool enable;
        public bool Enable
        {
            get
            {
                return enable;
            }
            set
            {
                SetProperty(ref enable, value);
            }
        }
    }
}
