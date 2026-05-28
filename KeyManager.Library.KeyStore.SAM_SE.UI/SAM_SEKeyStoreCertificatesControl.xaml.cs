/*
** File Name: SAM_SEKeyStoreCertificatesControl.cs
** Author: s_eva
** Creation date: September 2024
** Description: This file corresponds of the .cs hidden behind SAM_SEKeyStoreCertificatesControl.xaml.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using log4net;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;


namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI
{
    /// <summary>
    /// Interaction logic for SAMKeyStoreCertificatesControl.xaml
    /// </summary>
    public partial class SAM_SEKeyStoreCertificatesControl : UserControl
    {
        public SAM_SEKeyStoreCertificatesControl()
        {
            InitializeComponent();

            DataContext = new SAM_SEKeyStoreCertificatesControlViewModel();

        }

        private void UserControl_Load(Object sender, EventArgs e)
        {
            var VM = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;
            VM?.UpdateAllCertificates();
        }

        private async void UserControl_Unload(Object sender, EventArgs e)
        {
            var VM = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;
            if (VM!.UpdateRequired)
                await VM!.AskForUpdate(VM);
            else
                return;
        }

        private void CertificatesDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.All;
            else
                e.Effects = DragDropEffects.None;
        }

        private async void CertificatesDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    if(sender is Grid grid)
                    {
                        var VM = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;
                        DLL.SAM_SECertificatesType type;
                        switch (grid.Name)
                        {
                            case "DropCAInt":
                                type = DLL.SAM_SECertificatesType.CERT_TYPE_CRT_CA;
                                break;
                            case "DropRCA":
                                type = DLL.SAM_SECertificatesType.CERT_TYPE_CRT_RCA;
                                break;
                            case "DropCA":
                                SAM_SEKeyStoreCertificatesProperties? properties = grid.DataContext as SAM_SEKeyStoreCertificatesProperties;
                                if(properties != null)
                                    type = properties.Type;
                                else
                                    type = DLL.SAM_SECertificatesType.CERT_TYPE_CRT_RCA;
                                break;
                            default:
                                type = DLL.SAM_SECertificatesType.CERT_TYPE_CRT_RCA;
                                break;
                        }
                        foreach (String file in files)
                        {
                            if (file.EndsWith(".pfx"))
                            {
                                await VM!.AskForPassword(VM);
                                VM!.AddFileToArchive(type, file, true);
                            }
                            else
                                VM!.AddFileToArchive(type, file, false);
                        }
                    }
                }
            }
        }

        private async void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Multiselect = true;

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            var VM = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;

            // Process open file dialog box results
            if (result == true)
            {
                //We search for the origin of the click
                string? name;
                SAM_SEKeyStoreCertificatesProperties? properties;

                if (sender is Button button)
                {
                    name = button.Name;
                    properties = button.DataContext as SAM_SEKeyStoreCertificatesProperties;
                }
                else if (sender is Rectangle rect)
                {
                    name = rect.Name;
                    properties = rect.DataContext as SAM_SEKeyStoreCertificatesProperties;
                }
                else
                { 
                    name = string.Empty;
                    properties = null;
                }
                //Then we search for the type of Certificate
                DLL.SAM_SECertificatesType type;

                if (name.StartsWith("BrowseCAInt"))
                {
                    type = DLL.SAM_SECertificatesType.CERT_TYPE_CRT_CA;
                }
                else if (name.StartsWith("BrowseRCA"))
                {
                    type = DLL.SAM_SECertificatesType.CERT_TYPE_CRT_RCA;
                }
                else if (name.StartsWith("BrowseCA"))
                {
                    if (properties != null)
                        type = properties.Type;
                    else
                        type = DLL.SAM_SECertificatesType.CERT_TYPE_CRT_RCA;
                }
                else
                {
                    type = DLL.SAM_SECertificatesType.CERT_TYPE_CRT_RCA;
                }
                //And we add the file to the archive
                foreach (String file in dialog.FileNames)
                {
                    if(file.EndsWith(".pfx"))
                    {
                        await VM!.AskForPassword(VM);
                        VM!.AddFileToArchive(type, file, true);
                    }
                    else
                        VM!.AddFileToArchive(type, file, false);
                }
            }
        }

        private async void BtnCSR_Click(object sender, RoutedEventArgs e)
        {
            var VM = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;

            await VM!.AskForSAN(VM);

            if (VM!.SANCancel == true)
                return;

            // Configure open file dialog box
            var dialog = new Microsoft.Win32.SaveFileDialog();

            // Filter by only .csr files
            dialog.Filter = "Request files (*.csr)|*.csr";
            dialog.FilterIndex = 1;

            Button button = (Button)sender;

            SAM_SEKeyStoreCertificatesProperties? properties = button.DataContext as SAM_SEKeyStoreCertificatesProperties;
            if (properties != null)
            {
                switch (button.Name)
                {
                    case "CsrCA":
                        switch(properties.Type)
                        {
                            case SAM_SECertificatesType.CERT_TYPE_CRT_RADIUS:
                                dialog.FileName = "CSR_" + VM!.CUName + "_RADIUS";
                                break;
                            case SAM_SECertificatesType.CERT_TYPE_CRT_TLS:
                                dialog.FileName = "CSR_" + VM!.CUName + "_TLS";
                                break;
                            case SAM_SECertificatesType.CERT_TYPE_CRT_WEB:
                                dialog.FileName = "CSR_" + VM!.CUName + "_WEB";
                                break;
                            case SAM_SECertificatesType.CERT_TYPE_CRT_JANUS:
                                dialog.FileName = "CSR_" + VM!.CUName + "_JANUS";
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                if (properties != null)
                {
                    switch (button.Name)
                    {
                        case "CsrCA":
                            VM!.GenerateCSR(properties.Type, dialog.FileName, VM!.SANToSendToDLL, VM!.SANNumber);
                            for (int i = 0; i < VM!.SANNumber; i++)
                                VM!.SANToSendToDLL[i] = string.Empty;
                            VM!.SANNumber = 0;
                            VM!.SANTypeSelected = "DNS";
                            break;
                        default:
                        break;
                    }
                }
            }
        }

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var fbdm = new FolderBrowserDialogViewModel();
            var fbd = new FolderBrowserDialog();
            fbd.DataContext = fbdm;

            var VM = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;

            // Process open file dialog box results
            if (fbd.ShowDialog() == true)
            {
                if (sender is Button button)
                {
                    SAM_SEKeyStoreCertificatesProperties? properties = button.DataContext as SAM_SEKeyStoreCertificatesProperties;
                    if (properties != null)
                    {
                        switch (button.Name)
                        {
                            case "DownloadCAInt":
                                VM!.DownloadCertificates(SAM_SECertificatesType.CERT_TYPE_CRT_CA, fbdm.SelectedFolder ?? "", properties.Id);
                                break;
                            case "DownloadRCA":
                                VM!.DownloadCertificates(SAM_SECertificatesType.CERT_TYPE_CRT_RCA, fbdm.SelectedFolder ?? "", 0);
                                break;
                            case "DownloadCA":
                                VM!.DownloadCertificates(properties.Type, fbdm.SelectedFolder ?? "", 0);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var VM = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;

            if (sender is Button button)
            {
                String msg = String.Empty;
                switch (button.Name)
                {
                    case "DeleteCAIntAll":
                        msg = Properties.Resources.DeleteCAIntAll;
                        break;
                    case "DeleteCAInt":
                        msg = Properties.Resources.DeleteCAInt;
                        break;
                    case "DeleteRCA":
                        msg = Properties.Resources.DeleteRCA;
                        break;
                    case "DeleteCA":
                        msg = Properties.Resources.DeleteCertificates;
                        break;
                    default:
                        break;
                }

                MessageBoxResult confirmResult = MessageBox.Show(msg, Properties.Resources.DeleteConfirmation, MessageBoxButton.YesNo);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    SAM_SEKeyStoreCertificatesProperties? properties = button.DataContext as SAM_SEKeyStoreCertificatesProperties;
                    switch (button.Name)
                    {
                        case "DeleteCAIntAll":
                            VM!.DeleteAllFilesFromArchive(SAM_SECertificatesType.CERT_TYPE_CRT_CA);
                            break;
                        case "DeleteCAInt":
                            if (properties != null)
                                VM!.DeleteFileFromArchive(properties.Type, properties.Id);
                            break;
                        case "DeleteRCA":
                            VM!.DeleteFileFromArchive(SAM_SECertificatesType.CERT_TYPE_CRT_RCA, 0);
                            break;
                        case "DeleteCA":
                            if (properties != null)
                            {
                                //Type is always the CRT file, so we use 2 delete to delete the KEY too
                                VM!.DeleteFileFromArchive(properties.Type, 0);
                                VM!.DeleteFileFromArchive(properties.Type+1, 0);
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            var VM = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;

            MessageBoxResult confirmResult = MessageBox.Show(Properties.Resources.UpdateConfirmationMessage, Properties.Resources.UpdateConfirmation, MessageBoxButton.YesNo);

            if (confirmResult == MessageBoxResult.Yes)
                VM!.UpdateCertificatesSAM_SE();
        }

        private void OnCardDoubleClick(object sender, MouseEventArgs e)
        {
            var VM = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;
            // Check which item was double-clicked
            if (sender is ListView listView)
            {
                SAM_SEKeyStoreCertificatesProperties? selectedItem = listView.SelectedItem as SAM_SEKeyStoreCertificatesProperties;

                if (selectedItem != null && selectedItem.Real == true && selectedItem.ValidCertificate == true)
                {
                    VM!.LaunchCRTFile(selectedItem.Type, selectedItem.Id);
                }
            }
        }

        [GeneratedRegex("(^\\d|\\.)$")]
        private static partial Regex IPRegex();

        private void IPValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            //Make sure it's a digit or a '.'
            Regex regex = IPRegex();
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}
