/*
** File Name: SAM_SESymmetricKeyEntryDESFireUIDPropertiesControl.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file corresponds of the .cs hidden behind SAM_SESymmetricKeyEntryDESFireUIDPropertiesControl.xaml.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI
{
    /// <summary>
    /// Interaction logic for SAMSymmetricKeyEntryDESFireUIDPropertiesControl.xaml
    /// </summary>
    public partial class SAM_SESymmetricKeyEntryDESFireUIDPropertiesControl : UserControl
    {
        public SAM_SESymmetricKeyEntryDESFireUIDPropertiesControl()
        {
            InitializeComponent();

            DataContext = new SAM_SESymmetricKeyEntryDESFireUIDPropertiesControlViewModel();
        }
    }
}
