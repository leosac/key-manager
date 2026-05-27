/*
** File Name: SAM_SEKeyStoreToolsControlViewModel.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file is the MVVM component of the KeyStore Tools.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.UI.Domain;
using System.Collections.ObjectModel;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL;
using System.Text.RegularExpressions;
using Leosac.WpfApp;
using System.Diagnostics;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.Properties;
using MaterialDesignThemes.Wpf;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain
{
    public partial class SAM_SEKeyStoreCertificatesControlViewModel : KeyStoreAdditionalControlViewModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public SAM_SEKeyStoreCertificatesControlViewModel()
        {
            CAInt = new();
            RCA = new();
            Certificates = new();
            SAN = new();
            SANType = ["DNS", "IP", "email", "URI"];
            SANTypeSelected = SANType[0];
            SANToSendToDLL = new string[16];
            AddRCA("", Properties.Resources.DragDrop, false, false);
            AddCertificates("", Properties.Resources.DragDrop, false, false, SAM_SECertificatesType.CERT_TYPE_CRT_RADIUS, false);
            AddCertificates("", Properties.Resources.DragDrop, false, false, SAM_SECertificatesType.CERT_TYPE_CRT_TLS, false);
            AddCertificates("", Properties.Resources.DragDrop, false, false, SAM_SECertificatesType.CERT_TYPE_CRT_WEB, false);
            AddCertificates("", Properties.Resources.DragDrop, false, false, SAM_SECertificatesType.CERT_TYPE_CRT_JANUS, false);
            NotEmptyCAInt = false;
        }

        //Lien regex IPv4 : https://stackoverflow.com/questions/5284147/validating-ipv4-addresses-with-regexp
        [GeneratedRegex("^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)\\.?\\b){4}$")]
        private static partial Regex IpRegex();

        public ObservableCollection<SAM_SEKeyStoreCertificatesProperties> CAInt { get; set; }
        public ObservableCollection<SAM_SEKeyStoreCertificatesProperties> RCA { get; set; }
        public ObservableCollection<SAM_SEKeyStoreCertificatesProperties> Certificates { get; set; }
        public ObservableCollection<string> SAN { get; set; }
        public ObservableCollection<string> SANType { get; }
        public string SANTypeSelected { get; set; }
        public string SANSelected { get; set; } = string.Empty;

        private string? _SANValue = string.Empty;
        public string? SANValue
        {
            get => _SANValue;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _SANValue, value);
                }
            }
        }

        public string?[] SANToSendToDLL { get; set; }
        public uint SANNumber { get; set; } = 0;
        public bool SANCancel { get; set; } = false;

        private bool _NotEmptyCAInt = false;
        public bool NotEmptyCAInt
        { 
            get => _NotEmptyCAInt; 
            set
            {
                SetProperty(ref _NotEmptyCAInt, value);
            }
        }

        private string? _CUName = "UGL";
        public string? CUName
        {
            get
            {
                _CUName = SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetCUNameArchiveIni();
                return _CUName;
            }
            set
            {
                SetProperty(ref _CUName, value);
                SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.SetCUNameArchiveIni(value);
                UpdateRequired = true;
            }
        }
        private uint _CUId = 1;
        public uint CUId
        { 
            get
            {
                _CUId = SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetCUIdArchiveIni();
                return _CUId;
            }
            set
            {
                SetProperty(ref _CUId, value);
                SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.SetCUIdArchiveIni(value);
                UpdateRequired = true;
            }
        }

        private string? _CUIp = "10.10.0.10";
        public string? CUIp
        {
            get
            {
                _CUIp = SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetCUIpArchiveIni();
                return _CUIp;
            }
            set
            {
                Regex regex = IpRegex();
                if (value != null)
                {
                    if (regex.IsMatch(value) || value == String.Empty)
                    {
                        SetProperty(ref _CUIp, value);
                        SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.SetCUIpArchiveIni(value);
                        UpdateRequired = true;
                    }
                }
            }
        }

        private string? _CUMask = "255.255.0.0";
        public string? CUMask
        {
            get
            {
                _CUMask = SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetCUMaskArchiveIni();
                return _CUMask;
            }
            set
            {
                Regex regex = IpRegex();
                if (value != null)
                {
                    if (regex.IsMatch(value) || value == String.Empty)
                    {
                        SetProperty(ref _CUMask, value);
                        SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.SetCUMaskArchiveIni(value);
                        UpdateRequired = true;
                    }
                }
            }
        }

        private string? _CUGateway = "";
        public string? CUGateway
        {
            get
            {
                _CUGateway = SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetCUGatewayArchiveIni();
                return _CUGateway;
            }
            set
            {
                Regex regex = IpRegex();
                if (value != null)
                {
                    if (regex.IsMatch(value) || value == String.Empty)
                    {
                        SetProperty(ref _CUGateway, value);
                        SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.SetCUGatewayArchiveIni(value);
                        UpdateRequired = true;
                    }
                }
            }
        }

        private string? _ACSIp = "10.10.0.1";
        public string? ACSIp
        {
            get
            {
                _ACSIp = SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetAccessControlIpArchiveIni();
                return _ACSIp;
            }
            set
            {
                Regex regex = IpRegex();
                if (value != null)
                {
                    if (regex.IsMatch(value) || value == String.Empty)
                    {
                        SetProperty(ref _ACSIp, value);
                        SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.SetAccessControlIpArchiveIni(value);
                        UpdateRequired = true;
                    }
                }
            }
        }

        private uint _FDLPort = 2036;
        public uint FDLPort
        {
            get
            {
                _FDLPort = SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetCUServerPortArchiveIni();
                return _FDLPort;
            }
            set
            {
                SetProperty(ref _FDLPort, value);
                SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.SetCUServerPortArchiveIni(value);
                UpdateRequired = true;
            }
        }

        private uint _UpdatePort = 2035;
        public uint UpdatePort
        {
            get
            {
                _UpdatePort = SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetTLSServerPortArchiveIni();
                return _UpdatePort;
            }
            set
            {
                SetProperty(ref _UpdatePort, value);
                SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.SetTLSServerPortArchiveIni(value);
                UpdateRequired = true;
            }
        }

        private string? _RadiusIdentity = "";
        public string? RadiusIdentity
        {
            get
            {
                _RadiusIdentity = SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetRadiusIdentityArchiveIni();
                return _RadiusIdentity;
            }
            set
            {
                SetProperty(ref _RadiusIdentity, value);
                SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.SetRadiusIdentityArchiveIni(value);
                UpdateRequired = true;
            }
        }

        private string? _PasswordPFX = string.Empty;
        public string? PasswordPFX
        {
            get
            {
                return _PasswordPFX;
            }
            set
            {
                SetProperty(ref _PasswordPFX, value);
            }
        }
        
        private bool _PasswordPFXValid = false;
        public bool PasswordPFXValid
        {
            get => _PasswordPFXValid;
            set => SetProperty(ref _PasswordPFXValid, value);
        }

        private bool _UpdateRequired = false;
        public bool UpdateRequired
        {
            get => _UpdateRequired;
            set => SetProperty(ref _UpdateRequired, value);
        }

        public void AddFileToArchive(SAM_SECertificatesType type, string path, bool pfx)
        {
            try
            {
                if (!pfx || (pfx && PasswordPFXValid))
                    SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.AddArchiveFile(type, path, PasswordPFX);
                else
                    return;
                UpdateRequired = true;
                log.Info("Adding file succeeded.");
                if(pfx)
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.AddPFXSuccess);
                else if (type == SAM_SECertificatesType.CERT_TYPE_KEY_TLS || type == SAM_SECertificatesType.CERT_TYPE_KEY_RADIUS ||
                    type == SAM_SECertificatesType.CERT_TYPE_KEY_WEB || type == SAM_SECertificatesType.CERT_TYPE_KEY_JANUS)
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.AddKeySuccess);
                else
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.AddCertSuccess);
            }
            catch (Exception ex)
            {
                log.Error("Adding file failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
            finally
            {
                PasswordPFXValid = false;
                PasswordPFX = string.Empty;
                UpdateAllCertificates();
            }
        }

        public void DeleteFileFromArchive(SAM_SECertificatesType type, uint CAInt)
        {
            try
            {
                SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.DeleteArchiveFile(type, CAInt);
                UpdateRequired = true;
                log.Info("Deleting certificate succeeded.");
                if (type == SAM_SECertificatesType.CERT_TYPE_CRT_RCA || type == SAM_SECertificatesType.CERT_TYPE_CRT_CA)
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.DeleteCertSuccess);
                else
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.DeleteCertKeySuccess);
            }
            catch (Exception ex)
            {
                if (type == SAM_SECertificatesType.CERT_TYPE_CRT_CA)
                {
                    log.Error(string.Format("Deleting intermediate certificate n°{0} failed.", CAInt + 1), ex);
                }
                else
                {
                    log.Error("Deleting certificate failed.", ex);
                }
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
            finally
            {
                UpdateAllCertificates();
            }
        }

        public void DeleteAllFilesFromArchive(SAM_SECertificatesType type)
        {
            try
            {
                if (type != SAM_SECertificatesType.CERT_TYPE_CRT_CA)
                    return;
                uint nbCAInt = SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.IsFileInstalled(SAM_SECertificatesType.CERT_TYPE_CRT_CA);
                for (uint i = 0; i < nbCAInt; i++)
                {
                    SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.DeleteArchiveFile(type, 0);
                }
                UpdateRequired = true;
                log.Info("Deleting all intermediates certificates succeeded.");
                SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.DeleteAllCaIntSuccess);
            }
            catch (Exception ex)
            {
                log.Error("Deleting all intermediates certificates failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
            finally
            {
                UpdateAllCertificates();
            }
        }

        public void UpdateAllCertificates()
        {
            uint val = 0, oldVal = 0;
            for (int i = 0; i < (int)SAM_SECertificatesType.CERT_TYPE_NB; i++)
            {
                try
                {
                    oldVal = val;       //oldVal = CRT / val = KEY when we are in the certificate section (not RCA or Intermediates CA)
                    if (SAM_SEKeyStore == null || SAM_SEKeyStore!.GetSAM_SEDll() == null || SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation() == null)
                    {
                        SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.UpdateFailed);
                        return;
                    }
                    val = SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.IsFileInstalled((SAM_SECertificatesType)i);
                    switch ((SAM_SECertificatesType)i)
                    {
                        case SAM_SECertificatesType.CERT_TYPE_CRT_RCA:
                            if (val == 0)
                                ClearRCA();
                            else
                            {
                                AddRCA("", SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetCertificateCommonName(SAM_SECertificatesType.CERT_TYPE_CRT_RCA, 0), true, true);
                            }
                            break;
                        case SAM_SECertificatesType.CERT_TYPE_CRT_CA:
                            ClearCAInt();
                            if (val != 0)
                            {
                                for (uint j = 0; j < val; j++)
                                {
                                    //Recuperer le commonName de chaque CAInt avec son index
                                    AddCAInt(j, "", SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetCertificateCommonName(SAM_SECertificatesType.CERT_TYPE_CRT_CA, j), true, true);
                                }
                            }
                            break;
                        case SAM_SECertificatesType.CERT_TYPE_CRT_TLS:
                            break;
                        case SAM_SECertificatesType.CERT_TYPE_KEY_TLS:
                            if (oldVal != 0 || val != 0)        //oldVal = CRT / val = KEY
                            {
                                ReplaceCertificates("", SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetCertificateCommonName(SAM_SECertificatesType.CERT_TYPE_CRT_TLS, 0), oldVal != 0, val != 0, SAM_SECertificatesType.CERT_TYPE_CRT_TLS, true);
                            }
                            else
                            {
                                ReplaceCertificates("", Properties.Resources.DragDrop, false, false, SAM_SECertificatesType.CERT_TYPE_CRT_TLS, false);
                            }
                            break;
                        case SAM_SECertificatesType.CERT_TYPE_CRT_RADIUS:
                            break;
                        case SAM_SECertificatesType.CERT_TYPE_KEY_RADIUS:
                            if (oldVal != 0 || val != 0)        //oldVal = CRT / val = KEY
                            {
                                ReplaceCertificates("", SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetCertificateCommonName(SAM_SECertificatesType.CERT_TYPE_CRT_RADIUS, 0), oldVal != 0, val != 0, SAM_SECertificatesType.CERT_TYPE_CRT_RADIUS, true);
                            }
                            else
                            {
                                ReplaceCertificates("", Properties.Resources.DragDrop, false, false, SAM_SECertificatesType.CERT_TYPE_CRT_RADIUS, false);
                            }
                            break;
                        case SAM_SECertificatesType.CERT_TYPE_CRT_WEB:
                            break;
                        case SAM_SECertificatesType.CERT_TYPE_KEY_WEB:
                            if (oldVal != 0 || val != 0)        //oldVal = CRT / val = KEY
                            {
                                ReplaceCertificates("", SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetCertificateCommonName(SAM_SECertificatesType.CERT_TYPE_CRT_WEB, 0), oldVal != 0, val != 0, SAM_SECertificatesType.CERT_TYPE_CRT_WEB, true);
                            }
                            else
                            {
                                ReplaceCertificates("", Properties.Resources.DragDrop, false, false, SAM_SECertificatesType.CERT_TYPE_CRT_WEB, false);
                            }
                            break;
                        case SAM_SECertificatesType.CERT_TYPE_CRT_JANUS:
                            break;
                        case SAM_SECertificatesType.CERT_TYPE_KEY_JANUS:
                            if (oldVal != 0 || val != 0)        //oldVal = CRT / val = KEY
                            {
                                ReplaceCertificates("", SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GetCertificateCommonName(SAM_SECertificatesType.CERT_TYPE_CRT_JANUS, 0), oldVal != 0, val != 0, SAM_SECertificatesType.CERT_TYPE_CRT_JANUS, true);
                            }
                            else
                            {
                                ReplaceCertificates("", Properties.Resources.DragDrop, false, false, SAM_SECertificatesType.CERT_TYPE_CRT_JANUS, false);
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Getting common name failed.", ex);
                    SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
                }
            }
        }

        public void UpdateCertificatesSAM_SE()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (SAM_SEKeyStore == null || SAM_SEKeyStore!.GetSAM_SEDll() == null || SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation() == null)
                {
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.UpdateFailed);
                    return;
                }
                SAM_SEKeyStore.GetSAM_SEDll().GetCurrentProgrammingStation().SAM_SE.Certificates.UpdateArchive();
                log.Info("Updating SAM-SE archive succeeded.");
                SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.UpdateSuccess);
            }
            catch (Exception ex)
            {
                log.Error("Updating SAM-SE archive failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
            finally
            {
                UpdateAllCertificates();
                UpdateRequired = false;
                Mouse.OverrideCursor = null;
            }
        }

        public void DownloadCertificates(SAM_SECertificatesType type, string path, uint CAInt)
        {
            try
            {
                if (type == SAM_SECertificatesType.CERT_TYPE_KEY_TLS || type == SAM_SECertificatesType.CERT_TYPE_KEY_RADIUS ||
                    type == SAM_SECertificatesType.CERT_TYPE_KEY_WEB || type == SAM_SECertificatesType.CERT_TYPE_KEY_JANUS)
                    return;

                SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.ExtractArchiveFile(type, path, CAInt);
                log.Info("Downloading certificate succeeded.");
                SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.DLSuccess);
            }
            catch (Exception ex)
            {
                log.Error("Downloading certificate failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
        }

        public void GenerateCSR(SAM_SECertificatesType type, string path, string?[] san, uint nbSan)
        {
            try
            {
                if (type == SAM_SECertificatesType.CERT_TYPE_CRT_RCA || type == SAM_SECertificatesType.CERT_TYPE_CRT_CA)
                    return;
                SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.GenerationCSR(type, path, san, nbSan);
                UpdateRequired = true;
                log.Info("Generating CSR succeeded.");
                SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.CSRSuccess);
            }
            catch (Exception ex)
            {
                log.Error("Generating CSR failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
            finally
            {
                UpdateAllCertificates();
            }
        }

        public void LaunchCRTFile(SAM_SECertificatesType type, uint CAInt)
        {
            try
            {
                string path = System.IO.Path.GetTempPath();
                path += SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.Certificates.ExtractArchiveFile(type, path, CAInt);
                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
                //Sleep otherwise the file is not launching, why ?
                Thread.Sleep(200);
                System.IO.File.Delete(path);    //Cleanup after
            }
            catch (Exception ex)
            {
                log.Error("launching CRT file failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
        }

        public async Task<bool> AskForPassword(SAM_SEKeyStoreCertificatesControlViewModel CertVm)
        {
            var dialog = new SAM_SECertificatesAskPasswordControl
            {
                DataContext = CertVm
            };
            var ret = await DialogHost.Show(dialog, "RootDialog");
            return (ret != null);
        }

        public async Task<bool> AskForUpdate(SAM_SEKeyStoreCertificatesControlViewModel CertVm)
        {
            var dialog = new SAM_SECertificatesAskUpdateControl
            {
                DataContext = CertVm
            };
            var ret = await DialogHost.Show(dialog, "RootDialog");
            return (ret != null);
        }

        public async Task<bool> AskForSAN(SAM_SEKeyStoreCertificatesControlViewModel CertVm)
        {
            var dialog = new SAM_SECertificatesAskSANControl
            {
                DataContext = CertVm
            };
            var ret = await DialogHost.Show(dialog, "RootDialog");
            return (ret != null);
        }

        public void CloseDialogHost()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        public void DisplaySnackBar(string msg)
        {
            SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, msg);
        }

        private void AddCAInt(uint id, string path, string? commonName, bool valid, bool real)
        {
            CAInt.Add(new SAM_SEKeyStoreCertificatesProperties(id, path, commonName, valid, SAM_SECertificatesType.CERT_TYPE_CRT_CA, real));
            NotEmptyCAInt = true;
        }

        private void AddRCA(string path, string? commonName, bool valid, bool real)
        {
            if (RCA.Count != 0)
            {
                SAM_SEKeyStoreCertificatesProperties? FakeCert = RCA.First();
                if (FakeCert.Real == false)
                    RCA.Remove(FakeCert);

                if (RCA.Count >= 1)
                {
                    RCA.Clear();
                }
            }

            RCA.Add(new SAM_SEKeyStoreCertificatesProperties(0, path, commonName, valid, SAM_SECertificatesType.CERT_TYPE_CRT_RCA, real));
        }

        private void AddCertificates(string path, string? commonName, bool validCert, bool validKey, SAM_SECertificatesType type, bool real)
        {
            Certificates.Add(new SAM_SEKeyStoreCertificatesProperties(0, path, commonName, validCert, validKey, type, real));
        }

        private void ReplaceCertificates(string path, string? commonName, bool validCert, bool validKey, SAM_SECertificatesType type, bool real)
        {
            int index = RemoveCertificates(type);
            if (index == -1)
                return;
            Certificates.Insert(index, new SAM_SEKeyStoreCertificatesProperties(0, path, commonName, validCert, validKey, type, real));
        }

        private int RemoveCertificates(SAM_SECertificatesType type)
        {
            foreach(SAM_SEKeyStoreCertificatesProperties cert in Certificates)
            {
                if (cert.Type == type)
                {
                    int ret = Certificates.IndexOf(cert);
                    Certificates.Remove(cert);
                    return ret;
                }
            }
            return -1;
        }

        private void ClearCAInt()
        {
            CAInt.Clear();
            NotEmptyCAInt = false;
        }

        private void ClearRCA()
        {
            RCA.Clear();
            AddRCA("", Properties.Resources.DragDrop, false, false);
        }

        private SAM_SEKeyStore? SAM_SEKeyStore
        {
            get { return KeyStore as SAM_SEKeyStore; }
        }
    }
}
