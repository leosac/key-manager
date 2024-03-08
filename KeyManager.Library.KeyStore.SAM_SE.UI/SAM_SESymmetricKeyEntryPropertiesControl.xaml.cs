/*
** File Name: SAM_SESymmetricKeyEntryPropertiesControl.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file corresponds of the .cs hidden behind SAM_SESymmetricKeyEntryPropertiesControl.xaml.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI
{
    /// <summary>
    /// Interaction logic for SAMSymmetricKeyEntryPropertiesControl.xaml
    /// </summary>
    public partial class SAM_SESymmetricKeyEntryPropertiesControl : UserControl
    {
        public SAM_SESymmetricKeyEntryPropertiesControl()
        {
            InitializeComponent();

            DataContext = new SAM_SESymmetricKeyEntryPropertiesControlViewModel();
        }

        [GeneratedRegex("[a-fA-F0-9]")]
        private static partial Regex HexaRegex();

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = HexaRegex();
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void Input_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut)
            {
                e.Handled = true;
            }
        }
    }
}
