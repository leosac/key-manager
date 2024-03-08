/*
** File Name: SAM_SESymmetricKeyEntryPropertiesDESFireDiv.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file regroups all properties of Diversification of a DESFire SAM-SE Key Entry.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SESymmetricKeyEntryPropertiesDESFireDiv : ObservableValidator
    {
        public bool UidLinkEnable { get; set; } = false;
        public uint UidLinkKeyNum { get; set; } = 0;
        public string? UidLinkId { get; set; } = string.Empty;
        public bool AidInverted { get; set; } = false;

        private int siLenMax = 40;
        public int SiLenMax 
        { 
            get => siLenMax;
            set
            {
                SetProperty(ref siLenMax, value);
            }
        }

        private bool enable = false;
        public bool Enable
        {
            get => enable;
            set
            {
                if (SetProperty(ref enable, value))
                {
                    UpdateExpanded();
                }
            }
        }

        private bool expanded = false;
        public bool Expanded
        {
            get => expanded;
            set
            {
                SetProperty(ref expanded, value);
            }
        }

        private bool keyInc = true;
        public bool KeyInc
        {
            get => keyInc;
            set
            {
                if (SetProperty(ref keyInc, value))
                    UpdateDESFireSiLenMax();
            }
        }

        private byte[] si = [];
        public byte[] Si
        {
            get => si;
            set
            {
                SetProperty(ref si, value);
            }
        }

        private string siString = string.Empty;
        public string SiString
        {
            get => siString;
            set
            {
                SetProperty(ref siString, value);
                if (value.Length % 2 != 0)
                {
                    WarningSi = true;
                    Si = [];
                }
                else
                {
                    WarningSi = false;
                    // Convert the hex string to bytes
                    byte[] bytes = new byte[value.Length/2];
                    for (int i = 0; i < value.Length / 2; i++)
                    {
                        // Parse the two characters of the hex string into a byte
                        string hexByte = value.Substring(i * 2, 2);
                        // Conversion to actual byte
                        bytes[i] = Convert.ToByte(hexByte, 16);
                    }
                    Si = bytes;
                }
            }
        }

        private bool warningSi = false;
        public bool WarningSi
        {
            get => warningSi;
            set
            {
                SetProperty(ref warningSi, value);
            }
        }

        private void UpdateExpanded()
        {
            if (Enable)
            {
                Expanded = true;
            }
            else
            {
                Expanded = false;
            }
        }
        private void UpdateDESFireSiLenMax()
        {
            if (!KeyInc)
            {
                SiLenMax = 42;
            }
            else
            {
                SiLenMax = 40;
            }
            if (Si.Length*2 > SiLenMax)
            {
                byte[] temp = Enumerable.Repeat((byte)0x00, SiLenMax/2).ToArray();
                Array.Copy(Si, temp, temp.Length);
                Si = temp;
            }
        }
    }
}
