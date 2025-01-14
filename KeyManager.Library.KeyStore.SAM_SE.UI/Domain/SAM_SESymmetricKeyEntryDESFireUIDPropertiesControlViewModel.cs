/*
** File Name: SAM_SESymmetricKeyEntryDESFireUIDPropertiesControlViewModel.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file is the MVVM component of the DESFire UID KeyEntry Properties.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.Plugin.UI.Domain;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Properties;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain
{
    public class SAM_SESymmetricKeyEntryDESFireUIDPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public SAM_SESymmetricKeyEntryDESFireUIDPropertiesControlViewModel()
        {
            Properties = new SAM_SESymmetricKeyEntryDESFireUIDProperties();
        }

        public SAM_SESymmetricKeyEntryDESFireUIDProperties? SAM_SEDESFireUIDProperties
        {
            get { return Properties as SAM_SESymmetricKeyEntryDESFireUIDProperties; }
        }

        public Dictionary<SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType, string> SAM_SE_Key_Entry_Types { get; } =
            new Dictionary<SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType, string>()
            {
                {SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFire, Resources.SAM_SEKeyEntryDESFire},
                {SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFireUID, Resources.SAM_SEKeyEntryDESFireUid},
                {SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.Authenticate, Resources.SAM_SEKeyEntryAuthenticate},
            };
    }
}
