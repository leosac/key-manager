/*
** File Name: SAM_SESymmetricKeyEntryAuthenticationPropertiesControl.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file corresponds of the .cs hidden behind SAM_SESymmetricKeyEntryAuthenticationPropertiesControl.xaml.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI
{
    /// <summary>
    /// Interaction logic for SAMSymmetricKeyEntryAuthenticationPropertiesControl.xaml
    /// </summary>
    public partial class SAM_SESymmetricKeyEntryReaderPropertiesControl : UserControl
    {
        public SAM_SESymmetricKeyEntryReaderPropertiesControl()
        {
            InitializeComponent();

            DataContext = new SAM_SESymmetricKeyEntryReaderPropertiesControlViewModel();
        }
    }
}
