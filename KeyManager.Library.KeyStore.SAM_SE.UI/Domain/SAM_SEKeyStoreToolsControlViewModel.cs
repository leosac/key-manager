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
        }

        public SAM_SEKeyStore? SAM_SEKeyStore
        {
            get { return KeyStore as SAM_SEKeyStore; }
        }

        public RelayCommand SAM_SEDefaultConfigFileCommand { get; }
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
