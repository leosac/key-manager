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
using Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain
{
    public class SAM_SESymmetricKeyEntryReaderPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public SAM_SESymmetricKeyEntryReaderPropertiesControlViewModel()
        {
            Properties = new SAM_SESymmetricKeyEntryReaderProperties();
        }

        public SAM_SESymmetricKeyEntryReaderProperties? SAM_SEReaderProperties
        {
            get { return Properties as SAM_SESymmetricKeyEntryReaderProperties; }
        }

        public Dictionary<SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType, string> SAM_SE_Key_Entry_Types { get; } =
            new Dictionary<SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType, string>()
            {
                {SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFire, Resources.SAM_SEKeyEntryDESFire},
                {SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.DESFireUID, Resources.SAM_SEKeyEntryDESFireUid},
                {SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.Authenticate, Resources.SAM_SEKeyEntryAuthenticate},
                {SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType.Reader, Resources.SAM_SEKeyEntryReader},
            };

        public Dictionary<SAM_SEReaderKeyType, string> SAM_SE_Key_Reader_Types { get; } =
            new Dictionary<SAM_SEReaderKeyType, string>()
            {
                        {SAM_SEReaderKeyType.KEY_READER_PERSONALIZED, Resources.SAM_SEKeyEntryTypePerso},
                        {SAM_SEReaderKeyType.KEY_READER_SYNCHRONIC, Resources.SAM_SEKeyEntryTypeSynchro},
            };
    }
}
