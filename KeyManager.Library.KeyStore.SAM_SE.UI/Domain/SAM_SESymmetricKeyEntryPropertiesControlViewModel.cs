/*
** File Name: SAM_SESymmetricKeyEntryPropertiesControlViewModel.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file is the MVVM component of the KeyEntry Properties.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.Plugin.UI.Domain;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Properties;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain
{
    public class SAM_SESymmetricKeyEntryPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public SAM_SESymmetricKeyEntryPropertiesControlViewModel()
        {
            Properties = new SAM_SESymmetricKeyEntryProperties();
        }

        public SAM_SESymmetricKeyEntryProperties? SAM_SEProperties
        {
            get { return Properties as SAM_SESymmetricKeyEntryProperties; }
        }

        public Dictionary<SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType, string> SAM_SE_Key_Entry_Types { get; } =
            new Dictionary<SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType, string>()
            {
                {SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFire, Resources.SAM_SEKeyEntryDESFire},
                {SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFireUID, Resources.SAM_SEKeyEntryDESFireUid},
                {SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.Authenticate, Resources.SAM_SEKeyEntryAuthenticate},
            };

        public Dictionary<SAM_SESymmetricKeyEntryPropertiesDESFire.SAM_SEDESFireMode, string> SAM_SE_DESFire_Mode_Text { get; } =
            new Dictionary<SAM_SESymmetricKeyEntryPropertiesDESFire.SAM_SEDESFireMode, string>()
            {
                {SAM_SESymmetricKeyEntryPropertiesDESFire.SAM_SEDESFireMode.Disable, Resources.DESFireModeDeactivated},
                {SAM_SESymmetricKeyEntryPropertiesDESFire.SAM_SEDESFireMode.IDP, Resources.DESFireModeIdp},
            };

        public Dictionary<SAM_SESymmetricKeyEntryPropertiesDESFire.SAM_SEDESFireEncryptMode, string> SAM_SE_DESFire_Encrypt { get; } =
            new Dictionary<SAM_SESymmetricKeyEntryPropertiesDESFire.SAM_SEDESFireEncryptMode, string>()
            {
                {SAM_SESymmetricKeyEntryPropertiesDESFire.SAM_SEDESFireEncryptMode.AES, Resources.DESFireCipherAes},
            };  

        public Dictionary<SAM_SESymmetricKeyEntryPropertiesDESFire.SAM_SEDESFireCommunicationMode, string> SAM_SE_DESFire_Communication { get; } =
            new Dictionary<SAM_SESymmetricKeyEntryPropertiesDESFire.SAM_SEDESFireCommunicationMode, string>()
            {
                {SAM_SESymmetricKeyEntryPropertiesDESFire.SAM_SEDESFireCommunicationMode.Encrypted, Resources.DESFireCommunicationEncrypted},
            };
    }
}
