/*
** File Name: SAM_SEKeyStorePropertiesControlViewModel.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file is the MVVM component of the KeyStore Properties.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.Properties;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain
{
    public class SAM_SEKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public SAM_SEKeyStorePropertiesControlViewModel()
        {
            _properties = new SAM_SEKeyStoreProperties();
            SAM_SEReaders = new ObservableCollection<string>();
            SAM_SEDll = new SAM_SEDllEntryPoint();
        }

        public SAM_SEKeyStoreProperties? SAM_SEProperties
        {
            get { return Properties as SAM_SEKeyStoreProperties; }
        }

        public ObservableCollection<string> SAM_SEReaders { get; set; }
        public SAM_SEDllEntryPoint SAM_SEDll { get; set; }

        public uint LockedMin { get; set; } = 0;
        public uint LockedMax { get; set; } = (uint)SAM_SELockLevel.LOCK_LVL_NB_MAX - 1;

        private int stationIndex = 0;
        public int SAM_SEStationIndex
        {
            get => stationIndex;
            set
            {
                if (SetProperty(ref stationIndex, value))
                {
                    log.Info(string.Format("Station Index : {0}", stationIndex));
                    try
                    {
                        //Stopping all the blinking leds
                        for (uint i = 0; i < SAM_SEDll.GetNumberProgrammingStation(); i++)
                        {
                            SAM_SEDll.BlinkLedId(i, 0);
                        }
                        //XAML can make stationIndex == -1 if nothing is selected
                        if (stationIndex >= 0)
                        {
                            //And making blink the selected one
                            SAM_SEDll.BlinkLedId((uint)stationIndex, 1);
                        }
                    }
                    //Catch do nothing, since we just want to catch the exception and .. do nothing about it
                    catch (Exception e)
                    {
                        //Well, at least let's log the error
                        log.Error(e);
                    }
                }
            }
        }

        private bool refreshList = true;
        public bool RefreshList
        {
            get => refreshList;
            set => SetProperty(ref refreshList, value);
        }

        private bool keyStorePropertiesErrorEnable = false;
        public bool KeyStorePropertiesErrorEnable
        {
            get => keyStorePropertiesErrorEnable;
            set => SetProperty(ref keyStorePropertiesErrorEnable, value);
        }

        private string programmingStation = string.Empty;
        public string ProgrammingStation
        {
            get => programmingStation;
            set
            {
                if (SetProperty(ref programmingStation, value))
                {
                    if (ProgrammingStation != null)
                    {
                        string[] stringParts = ProgrammingStation.Split(':');
                        //Extraction of MAC to identify the SAM-SE
                        if (uint.TryParse(stringParts[0].Split("°")[1], out uint index) == true)
                        {
                            log.Info(string.Format("Station Index gotten before -- : {0}", index));
                            //Index is incremented by 1 for a better user experience
                            index--;
                            SAM_SEProperties!.ProgrammingStationPath = SAM_SEDll.GetPath(index);
                        }
                    }
                    else
                    {
                        SAM_SEProperties!.ProgrammingStationPath = string.Empty;
                    }
                }
            }
        }

        public async Task RefreshStationProgrammationList()
        {
            try
            {
                RefreshList = false;
                string prevru = ProgrammingStation;
                SAM_SEReaders.Clear();
                Mouse.OverrideCursor = Cursors.Wait;
                await SAM_SEDll.DetectProgrammingStation();
                uint nb = SAM_SEDll.GetNumberProgrammingStation();
                for (uint i = 0; i < nb; i++)
                {
                    SAM_SEDll.BlinkLedId(i, 0);

                    if (SAM_SEDll.GetSAM_SEPresence(i) == SAM_SEConnectionState.STATION_CONNECTED_WITH_SAMSE)
                    {
                        string version = SAM_SEDll.GetVersion(i);
                        string mac = SAM_SEDll.GetMac(i);

                        SAM_SEReaders.Add(string.Format("Station n°{0} : SAM-SE : {1} - {2} - {3}", i + 1, "SE", mac, version));
                    }
                    else
                    {
                        SAM_SEReaders.Add(string.Format("Station n°{0} : SAM-SE : {1}", i+1, Resources.Absent));
                    }
                }
                log.Info(string.Format("Number of stations : {0}", SAM_SEReaders.Count));
                //If we find the old element which was selected before the refresh
                if (SAM_SEReaders.Contains(prevru))
                {
                    //We select it back and make the Led blink to notify the user
                    ProgrammingStation = prevru;
                    SAM_SEDll.BlinkLedId((uint)SAM_SEStationIndex, 1);
                    KeyStorePropertiesErrorEnable = false;
                }
                //If we don't find the old element, and if there is at least one programming station
                else if (SAM_SEReaders.Count != 0)
                {
                    //We select the first one of the list, and we make its Led blink
                    ProgrammingStation = SAM_SEReaders.First();
                    SAM_SEDll.BlinkLedId(0, 1);
                    KeyStorePropertiesErrorEnable = false;
                }
                //If there is no programming station, we put the string as empty
                else
                {
                    ProgrammingStation = string.Empty;
                    KeyStorePropertiesErrorEnable = true;
                }
            }
            catch (Exception ex)
            {
                KeyStorePropertiesErrorEnable = true;
                log.Error(ex);
            }
            finally
            {
                RefreshList = true;
                Mouse.OverrideCursor = null;
            }
        }
    }
}
