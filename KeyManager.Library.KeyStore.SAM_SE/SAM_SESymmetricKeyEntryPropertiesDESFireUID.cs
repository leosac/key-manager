/*
** File Name: SAM_SESymmetricKeyEntryPropertiesDESFireUID.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file regroups all properties of a DESFire UID SAM-SE Key Entry.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryPropertiesDESFireUID : ObservableValidator
    {
        private bool enable;
        public byte KeyNum { get; set; } = 0;
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
