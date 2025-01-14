/*
** File Name: SAM_SESymmetricKeyEntryDESFirePropertiesControlViewModel.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file is the MVVM component of the DESFire KeyEntry Properties.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.Plugin.UI.Domain;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Properties;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain
{
    public class SAM_SESymmetricKeyEntryDESFirePropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public SAM_SESymmetricKeyEntryDESFirePropertiesControlViewModel()
        {
            Properties = new SAM_SESymmetricKeyEntryDESFireProperties();
        }

        public SAM_SESymmetricKeyEntryDESFireProperties? SAM_SEDESFireProperties
        {
            get { return Properties as SAM_SESymmetricKeyEntryDESFireProperties; }
        }

        public Dictionary<SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType, string> SAM_SE_Key_Entry_Types { get; } =
            new Dictionary<SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType, string>()
            {
                {SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFire, Resources.SAM_SEKeyEntryDESFire},
                {SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFireUID, Resources.SAM_SEKeyEntryDESFireUid},
                {SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.Authenticate, Resources.SAM_SEKeyEntryAuthenticate},
            };

        public Dictionary<SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireMode, string> SAM_SE_DESFire_Mode_Text { get; } =
            new Dictionary<SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireMode, string>()
            {
                        {SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireMode.Disable, Resources.DESFireModeDeactivated},
                        {SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireMode.IDP, Resources.DESFireModeIdp},
            };

        public Dictionary<SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireEncryptMode, string> SAM_SE_DESFire_Encrypt { get; } =
            new Dictionary<SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireEncryptMode, string>()
            {
                        {SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireEncryptMode.AES, Resources.DESFireCipherAes},
            };

        public Dictionary<SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireCommunicationMode, string> SAM_SE_DESFire_Communication { get; } =
            new Dictionary<SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireCommunicationMode, string>()
            {
                {SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireCommunicationMode.Encrypted, Resources.DESFireCommunicationEncrypted},
            };
    }
}
