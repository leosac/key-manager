/*
** File Name: SAM_SESymmetricKeyEntryAuthenticationProperties.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file regroups all properties of a SAM-SE Authentication Key Entry.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryReaderProperties : SAM_SESymmetricKeyEntryProperties
    {
        public SAM_SESymmetricKeyEntryReaderProperties() : base()
        {
            KeyEntryType = SAM_SEKeyEntryType.Reader;
        }

        private bool _CurrentKeyActive = true;
        private bool _NewKeyActive = true;
        private SAM_SEReaderKeyType _CurrentKey = SAM_SEReaderKeyType.KEY_READER_SYNCHRONIC;
        private SAM_SEReaderKeyType _NewKey = SAM_SEReaderKeyType.KEY_READER_PERSONALIZED;

        public bool CurrentKeyActive
        {
            get { return _CurrentKeyActive; }
            set
            {
                SetProperty(ref _CurrentKeyActive, value);
            }
        }
        public bool NewKeyActive
        {
            get { return _NewKeyActive; }
            set
            {
                SetProperty(ref _NewKeyActive, value);
            }
        }
        public SAM_SEReaderKeyType CurrentKey
        {
            get { return _CurrentKey; }
            set
            {
                SetProperty(ref _CurrentKey, value);
            }
        }
        public SAM_SEReaderKeyType NewKey
        {
            get { return _NewKey; }
            set
            {
                SetProperty(ref _NewKey, value);
            }
        }
    }
}
