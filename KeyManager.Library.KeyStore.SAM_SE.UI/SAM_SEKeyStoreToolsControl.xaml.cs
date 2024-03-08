/*
** File Name: SAM_SEKeyStoreToolsControl.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file corresponds of the .cs hidden behind SAM_SEKeyStoreToolsControl.xaml.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI
{
    /// <summary>
    /// Interaction logic for SAMKeyStoreToolsControl.xaml
    /// </summary>
    public partial class SAM_SEKeyStoreToolsControl : UserControl
    {
        public SAM_SEKeyStoreToolsControl()
        {
            InitializeComponent();

            DataContext = new SAM_SEKeyStoreToolsControlViewModel();
        }
    }
}
