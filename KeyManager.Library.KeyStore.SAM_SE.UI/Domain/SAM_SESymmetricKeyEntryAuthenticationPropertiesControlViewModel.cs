/*
** File Name: SAM_SESymmetricKeyEntryAuthenticationPropertiesControlViewModel.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file is the MVVM component of the Authentication KeyEntry Properties.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.Plugin.UI.Domain;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Properties;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain
{
    public class SAM_SESymmetricKeyEntryAuthenticationPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public SAM_SESymmetricKeyEntryAuthenticationPropertiesControlViewModel()
        {
            Properties = new SAM_SESymmetricKeyEntryAuthenticationProperties();
        }

        public SAM_SESymmetricKeyEntryAuthenticationProperties? SAM_SEAuthenticationProperties
        {
            get { return Properties as SAM_SESymmetricKeyEntryAuthenticationProperties; }
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
