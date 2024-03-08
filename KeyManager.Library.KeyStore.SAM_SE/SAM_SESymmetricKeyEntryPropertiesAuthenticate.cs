/*
** File Name: SAM_SESymmetricKeyEntryPropertiesAuthenticate.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file regroups all properties of an Authenticate SAM-SE Key Entry.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryPropertiesAuthenticate : ObservableValidator
    {
        private string password = string.Empty;
        private string passwordConfirmation = string.Empty;
        private bool passwordValid = false;
        private bool passwordNotValid = true;
        private bool passwordMatch = false;
        private bool passwordNotMatch = true;
        private bool toolTipEnable = false;
        private string error = string.Empty;
        public string Password
        {
            get => password;
            set
            {
                SetProperty(ref password, value);
                PasswordValidity();
            }
        }
        public string PasswordConfirmation
        {
            get => passwordConfirmation;
            set
            {
                SetProperty(ref passwordConfirmation, value);
                PasswordMatching();
            }
        }
        public bool PasswordValid
        {
            get => passwordValid;
            set
            {
                SetProperty(ref passwordValid, value);
                PasswordNotValid = !passwordValid;
            }
        }        
        public bool PasswordNotValid
        {
            get => passwordNotValid;
            set
            {
                SetProperty(ref passwordNotValid, value);
            }
        }

        public bool PasswordMatch
        {
            get => passwordMatch;
            set
            {
                SetProperty(ref passwordMatch, value);
                PasswordNotMatch = !passwordMatch;
            }
        }
        public bool PasswordNotMatch
        {
            get => passwordNotMatch;
            set
            {
                SetProperty(ref passwordNotMatch, value);
            }
        }        
        public bool ToolTipEnable
        {
            get => toolTipEnable;
            set
            {
                SetProperty(ref toolTipEnable, value);
            }
        }
        public string Error
        {
            get => error;
            set
            {
                SetProperty(ref error, value);
            }
        }

        public void PasswordValidity()
        {
            if (Password == string.Empty)
            {
                PasswordValid = false;
                ToolTipEnable = false;
                return;
            }

            ToolTipEnable = true;

            if (Password.Length < 8)
            {
                PasswordValid = false;
                Error = Properties.Resources.PasswordErrorLength;
                throw new KeyStoreException(Properties.Resources.PasswordErrorLength);
            }

            int number = 0;
            int letter = 0;
            foreach (char carac in Password)
            {
                if (carac >= '0' && carac <= '9')
                    number++;
                else if ((carac >= 'a' && carac <= 'z') || (carac >= 'A' && carac <= 'Z'))
                    letter++;
            }
            if (letter == 0)
            {
                PasswordValid = false;
                Error = Properties.Resources.PasswordErrorLetter;
                throw new KeyStoreException(Properties.Resources.PasswordErrorLetter);
            }
            if (number == 0)
            {
                PasswordValid = false;
                Error = Properties.Resources.PasswordErrorDigit;
                throw new KeyStoreException(Properties.Resources.PasswordErrorDigit);
            }
            PasswordValid = true;
            PasswordMatching();
        }

        public void PasswordMatching()
        {
            if (PasswordValid)
            {
                if (Password == PasswordConfirmation)
                    PasswordMatch = true;
                else
                    PasswordMatch = false;
            }
            else
                PasswordMatch = false;
        }
    }
}
