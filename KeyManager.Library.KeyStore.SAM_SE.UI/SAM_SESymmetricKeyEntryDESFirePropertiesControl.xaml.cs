/*
** File Name: SAM_SESymmetricKeyEntryDESFirePropertiesControl.cs
** Author: s_eva
** Creation date: March 2024
** Description: This file corresponds of the .cs hidden behind SAM_SESymmetricKeyEntryDESFirePropertiesControl.xaml.
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
    /// Interaction logic for SAMSymmetricKeyEntryDESFirePropertiesControl.xaml
    /// </summary>
    public partial class SAM_SESymmetricKeyEntryDESFirePropertiesControl : UserControl
    {
        public SAM_SESymmetricKeyEntryDESFirePropertiesControl()
        {
            InitializeComponent();

            DataContext = new SAM_SESymmetricKeyEntryDESFirePropertiesControlViewModel();
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
