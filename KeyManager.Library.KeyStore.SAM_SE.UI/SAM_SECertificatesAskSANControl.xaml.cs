/*
** File Name: SAM_SECertificatesAskSANControl.cs
** Author: s_eva
** Creation date: September 2024
** Description: This file corresponds of the .cs hidden behind SAM_SECertificatesAskSANControl.xaml.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI
{
    /// <summary>
    /// Interaction logic for SAMKeyStoreCertificatesControl.xaml
    /// </summary>
    public partial class SAM_SECertificatesAskSANControl : UserControl
    {
        public SAM_SECertificatesAskSANControl()
        {
            InitializeComponent();
        }

        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            SAM_SEKeyStoreCertificatesControlViewModel? CertVm = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;
            if (CertVm != null)
            {
                CertVm.SAN.Clear();
                CertVm.SANTypeSelected = string.Empty;
                for (int i = 0; i < (int)SAM_SEMaxSAN.SAN_MAX_NUM; i++)
                    CertVm.SANToSendToDLL[i] = null;
                CertVm.SANNumber = 0;
                CertVm.SANCancel = true;
            }
        }

        private void Btn_Valid(object sender, RoutedEventArgs e)
        {
            SAM_SEKeyStoreCertificatesControlViewModel? CertVm = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;
            if (CertVm != null)
            {
                foreach (string san in CertVm.SAN)
                {
                    CertVm.SANToSendToDLL[CertVm.SANNumber] = san;
                    CertVm.SANNumber++;
                    if (CertVm.SANNumber >= (int)SAM_SEMaxSAN.SAN_MAX_NUM)
                        break;
                }
                CertVm.SAN.Clear();
                CertVm.SANTypeSelected = string.Empty;
                CertVm.SANCancel = false;
            }
        }
        
        private void Btn_Add(object sender, RoutedEventArgs e)
        {
            SAM_SEKeyStoreCertificatesControlViewModel? CertVm = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;
            if (CertVm != null)
            {
                if (CertVm.SAN.Count < (int)SAM_SEMaxSAN.SAN_MAX_NUM)
                    CertVm.SAN.Add(CertVm.SANTypeSelected + ":" + CertVm.SANValue);
                else
                    CertVm.DisplaySnackBar(Properties.Resources.TooMuchSan);
                CertVm.SANValue = string.Empty;
            }
        }

        private void Btn_Delete(object sender, RoutedEventArgs e)
        {
            SAM_SEKeyStoreCertificatesControlViewModel? CertVm = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;
            if (CertVm != null)
            {
                if (CertVm.SANSelected != string.Empty)
                {
                    CertVm.SAN.Remove(CertVm.SANSelected);
                    CertVm.SANSelected = string.Empty;
                }
            }
        }

        private void Key_Pressed(object sender, KeyEventArgs e)
        {
            
            SAM_SEKeyStoreCertificatesControlViewModel? CertVm = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;
            if (CertVm != null)
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    foreach (string san in CertVm.SAN)
                    {
                        CertVm.SANToSendToDLL[CertVm.SANNumber] = san;
                        CertVm.SANNumber++;
                        if (CertVm.SANNumber >= (int)SAM_SEMaxSAN.SAN_MAX_NUM)
                            break;
                    }
                    CertVm.SAN.Clear();
                    CertVm.SANTypeSelected = string.Empty;
                    CertVm.SANCancel = false;
                    CertVm.CloseDialogHost();
                }
                else if (e.Key == System.Windows.Input.Key.Escape)
                {
                    CertVm.SAN.Clear();
                    CertVm.SANTypeSelected = string.Empty;
                    for (int i = 0; i < (int)SAM_SEMaxSAN.SAN_MAX_NUM; i++)
                        CertVm.SANToSendToDLL[i] = null;
                    CertVm.SANNumber = 0;
                    CertVm.SANCancel = true;
                    CertVm.CloseDialogHost();
                }
            }
        }
    }
}
