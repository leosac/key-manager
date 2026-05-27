/*
** File Name: SAM_SEKeyStoreCertificatesControl.cs
** Author: s_eva
** Creation date: September 2024
** Description: This file corresponds of the .cs hidden behind SAM_SEKeyStoreCertificatesControl.xaml.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI
{
    /// <summary>
    /// Interaction logic for SAMKeyStoreCertificatesControl.xaml
    /// </summary>
    public partial class SAM_SECertificatesAskUpdateControl : UserControl
    {
        public SAM_SECertificatesAskUpdateControl()
        {
            InitializeComponent();
        }

        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            SAM_SEKeyStoreCertificatesControlViewModel? CertVm = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;
            if (CertVm != null)
            {
                CertVm.UpdateAllCertificates();
            }
        }

        private void Btn_Valid(object sender, RoutedEventArgs e)
        {
            SAM_SEKeyStoreCertificatesControlViewModel? CertVm = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;
            if (CertVm != null)
            {
                CertVm.UpdateCertificatesSAM_SE();
            }
        }
        private void Key_Pressed(object sender, KeyEventArgs e)
        {
            SAM_SEKeyStoreCertificatesControlViewModel? CertVm = DataContext as SAM_SEKeyStoreCertificatesControlViewModel;
            if (CertVm != null)
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    CertVm.UpdateCertificatesSAM_SE();
                    CertVm.CloseDialogHost();
                }
                else if (e.Key == System.Windows.Input.Key.Escape)
                {
                    CertVm.UpdateAllCertificates();
                    CertVm.CloseDialogHost();
                }
            }
        }
    }
}
