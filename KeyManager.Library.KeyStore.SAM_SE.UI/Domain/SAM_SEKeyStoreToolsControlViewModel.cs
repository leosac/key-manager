/*
** File Name: SAM_SEKeyStoreToolsControlViewModel.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file is the MVVM component of the KeyStore Tools.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Properties;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain
{
    public class SAM_SEKeyStoreToolsControlViewModel : KeyStoreAdditionalControlViewModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        public SAM_SEKeyStoreToolsControlViewModel()
        {
            SAM_SELockCommand = new RelayCommand(SAM_SELockState);
            SAM_SEDefaultConfigFileCommand = new RelayCommand(SAM_SECreateDefaultConfigFile);
            SAM_SEHardenedConfigFileCommand = new RelayCommand(SAM_SECreateHardenedConfigFile);
        }

        public SAM_SEKeyStore? SAM_SEKeyStore
        {
            get { return KeyStore as SAM_SEKeyStore; }
        }

        public RelayCommand SAM_SEDefaultConfigFileCommand { get; }
        public RelayCommand SAM_SEHardenedConfigFileCommand { get; }
        public RelayCommand SAM_SELockCommand { get; }

        private bool lockedLevelExpanded = false;
        public bool LockedLevelExpanded
        {
            get => lockedLevelExpanded;
            set => SetProperty(ref lockedLevelExpanded, value);
        }

        private bool defaultExpanderExpanded = false;
        public bool DefaultExpanderExpanded
        {
            get => defaultExpanderExpanded;
            set => SetProperty(ref defaultExpanderExpanded, value);
        }

        public uint LockedMin { get; set; } = 0;
        public uint LockedMax { get; set; } = (uint)SAM_SELockLevel.LOCK_LVL_NB_MAX - 1;

        private string configuration1 = Resources.DESFireModeUid;
        public string Configuration1
        {
            get => configuration1;
            set
            {
                if (SetProperty(ref configuration1, value))
                {
                    if (value == Resources.ConfFileDisabled)
                    {
                        Configuration2Displayed = false;
                    }
                    else
                    {
                        Configuration2Displayed = true;
                    }
                }
            }
        }

        private bool configuration2Displayed = true;
        public bool Configuration2Displayed
        {
            get => configuration2Displayed;
            set
            {
                if(SetProperty(ref configuration2Displayed, value))
                {
                    if (value == false)
                    {
                        Configuration3Displayed = false;
                        Configuration4Displayed = false;
                    }
                    else
                    {
                        Configuration3Displayed = true;
                        Configuration4Displayed = false;
                    }
                }
            }
        }

        private string configuration2 = Resources.ConfFileDisabled;
        public string Configuration2
        {
            get => configuration2;
            set
            {
                if (SetProperty(ref configuration2, value))
                {
                    if (value == Resources.ConfFileDisabled)
                    {
                        Configuration3Displayed = false;
                    }
                    else
                    {
                        Configuration3Displayed = true;
                    }
                }
            }
        }

        private bool configuration3Displayed = false;
        public bool Configuration3Displayed
        {
            get => configuration3Displayed;
            set
            {
                if (SetProperty(ref configuration3Displayed, value))
                {
                    if (value == false)
                    {
                        Configuration4Displayed = false;
                    }
                    else
                    {
                        Configuration4Displayed = true;
                    }
                }

            }
        }

        private string configuration3 = Resources.ConfFileDisabled;
        public string Configuration3
        {
            get => configuration3;
            set
            {
                if (SetProperty(ref configuration3, value))
                {
                    if (value == Resources.ConfFileDisabled)
                    {
                        Configuration4Displayed = false;
                    }
                    else
                    {
                        Configuration4Displayed = true;
                    }
                }
            }
        }

        private bool configuration4Displayed = false;
        public bool Configuration4Displayed
        {
            get => configuration4Displayed;
            set => SetProperty(ref configuration4Displayed, value);
        }

        private string configuration4 = Resources.ConfFileDisabled;
        public string Configuration4
        {
            get => configuration4;
            set
            {
                SetProperty(ref configuration4, value);
            }
        }

        private string configurationClassic = Resources.DESFireModeUid;
        public string ConfigurationClassic
        {
            get => configurationClassic;
            set
            {
                SetProperty(ref configurationClassic, value);
            }
        }

        private string configurationUltra = Resources.DESFireModeUid;
        public string ConfigurationUltra
        {
            get => configurationUltra;
            set
            {
                SetProperty(ref configurationUltra, value);
            }
        }

        private string configurationPlus = Resources.DESFireModeUid;
        public string ConfigurationPlus
        {
            get => configurationPlus;
            set
            {
                SetProperty(ref configurationPlus, value);
            }
        }

        private string configuration125k = Resources.DESFireModeUid;
        public string Configuration125k
        {
            get => configuration125k;
            set
            {
                SetProperty(ref configuration125k, value);
            }
        }
        
        private string configurationHardened1 = Resources.DESFireModeIdp;
        public string ConfigurationHardened1
        {
            get => configurationHardened1;
            set
            {
                if (SetProperty(ref configurationHardened1, value))
                {
                    if (value == Resources.ConfFileDisabled)
                    {
                        ConfigurationHardened2Displayed = false;
                    }
                    else
                    {
                        ConfigurationHardened2Displayed = true;
                    }
                }
            }
        }

        private bool configurationHardened2Displayed = true;
        public bool ConfigurationHardened2Displayed
        {
            get => configurationHardened2Displayed;
            set
            {
                if(SetProperty(ref configurationHardened2Displayed, value))
                {
                    if (value == false)
                    {
                        ConfigurationHardened3Displayed = false;
                        ConfigurationHardened4Displayed = false;
                    }
                    else
                    {
                        ConfigurationHardened3Displayed = true;
                        ConfigurationHardened4Displayed = false;
                    }
                }
            }
        }

        private string configurationHardened2 = Resources.ConfFileDisabled;
        public string ConfigurationHardened2
        {
            get => configurationHardened2;
            set
            {
                if (SetProperty(ref configurationHardened2, value))
                {
                    if (value == Resources.ConfFileDisabled)
                    {
                        ConfigurationHardened3Displayed = false;
                    }
                    else
                    {
                        ConfigurationHardened3Displayed = true;
                    }
                }
            }
        }

        private bool configurationHardened3Displayed = false;
        public bool ConfigurationHardened3Displayed
        {
            get => configurationHardened3Displayed;
            set
            {
                if (SetProperty(ref configurationHardened3Displayed, value))
                {
                    if (value == false)
                    {
                        ConfigurationHardened4Displayed = false;
                    }
                    else
                    {
                        ConfigurationHardened4Displayed = true;
                    }
                }

            }
        }

        private string configurationHardened3 = Resources.ConfFileDisabled;
        public string ConfigurationHardened3
        {
            get => configurationHardened3;
            set
            {
                if (SetProperty(ref configurationHardened3, value))
                {
                    if (value == Resources.ConfFileDisabled)
                    {
                        ConfigurationHardened4Displayed = false;
                    }
                    else
                    {
                        ConfigurationHardened4Displayed = true;
                    }
                }
            }
        }

        private bool configurationHardened4Displayed = false;
        public bool ConfigurationHardened4Displayed
        {
            get => configurationHardened4Displayed;
            set => SetProperty(ref configurationHardened4Displayed, value);
        }

        private string configurationHardened4 = Resources.ConfFileDisabled;
        public string ConfigurationHardened4
        {
            get => configurationHardened4;
            set
            {
                SetProperty(ref configurationHardened4, value);
            }
        }

        private string configurationHardenedClassic = Resources.ConfFileDisabled;
        public string ConfigurationHardenedClassic
        {
            get => configurationHardenedClassic;
            set
            {
                SetProperty(ref configurationHardenedClassic, value);
            }
        }

        private string configurationHardenedUltra = Resources.ConfFileDisabled;
        public string ConfigurationHardenedUltra
        {
            get => configurationHardenedUltra;
            set
            {
                SetProperty(ref configurationHardenedUltra, value);
            }
        }

        private string configurationHardenedPlus = Resources.ConfFileDisabled;
        public string ConfigurationHardenedPlus
        {
            get => configurationHardenedPlus;
            set
            {
                SetProperty(ref configurationHardenedPlus, value);
            }
        }

        private string configurationHardened125k = Resources.ConfFileDisabled;
        public string ConfigurationHardened125k
        {
            get => configurationHardened125k;
            set
            {
                SetProperty(ref configurationHardened125k, value);
            }
        }

        public string VersionSAM_SE
        {
            get { return string.Format("{0} : {1}", Resources.VersionSAM, SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.GetVersionSAM_SE()); }
        }
        public string VersionUT
        {
            get { return string.Format("{0} : {1}", Resources.VersionUT, SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.GetVersionUT()); }
        }
        public string VersionUGL
        {
            get { return string.Format("{0} : {1}", Resources.VersionUGL, SAM_SEKeyStore!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.GetVersionUGL()); }
        }

        private async void SAM_SECreateDefaultConfigFile()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                var ks = (KeyStore as SAM_SEKeyStore);
                ks?.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.SetDefaultConfigurationFile();
                await ks!.GetAll();
                SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.ToolsConfFileDefaultOk);
                DefaultExpanderExpanded = false;
            }
            catch (Exception ex)
            {
                log.Error("Creating default file failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async void SAM_SECreateHardenedConfigFile()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                var ks = (KeyStore as SAM_SEKeyStore);
                ks?.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.SetHardenedConfigurationFile();
                await ks!.GetAll();
                SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.ToolsConfFileHardenedOk);
                DefaultExpanderExpanded = false;
            }
            catch (Exception ex)
            {
                log.Error("Creating hardened file failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void SAM_SELockState()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var ks = (KeyStore as SAM_SEKeyStore);
            try
            {
                ks?.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.SetLockLevel(ks!.GetFileProperties().LockedLevel);
                SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, Resources.ToolsLockLevelOk);
            }
            catch (Exception ex)
            {
                log.Error("Changing lock level failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
                ks!.GetFileProperties().LockedLevel = ks!.GetSAM_SEDll()!.GetCurrentProgrammingStation()!.SAM_SE.GetLockLevel();
                DefaultExpanderExpanded = true;
            }
            finally
            {
                LockedLevelExpanded = false;
                Mouse.OverrideCursor = null;
            }
        }
    }
}
