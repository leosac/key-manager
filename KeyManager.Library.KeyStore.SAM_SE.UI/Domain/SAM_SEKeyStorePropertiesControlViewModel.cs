/*
** File Name: SAM_SEKeyStorePropertiesControlViewModel.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file is the MVVM component of the KeyStore Properties.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
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
            SAM_SEDll = new SAM_SEDllProgrammingStation();
        }

        public SAM_SEKeyStoreProperties? SAM_SEProperties
        {
            get { return Properties as SAM_SEKeyStoreProperties; }
        }

        public ObservableCollection<string> SAM_SEReaders { get; set; }
        public SAM_SEDllProgrammingStation SAM_SEDll { get; set; }

        private int stationIndex = 0;
        public int SAM_SEStationIndex
        {
            get => stationIndex;
            set
            {
                if (SetProperty(ref stationIndex, value))
                {
                    try
                    {
                        //Stopping all the blinking leds
                        for (uint i = 0; i < SAM_SEDll.GetNumberSAM_SE(); i++)
                        {
                            SAM_SEDll.BlinkLedId(i, 0);
                        }
                        //XAML can make stationIndex == -1 if nothing is selected
                        if (stationIndex > 0)
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

        private bool keystorePropertiesErrorEnable = false;
        public bool KeyStorePropertiesErrorEnable
        {
            get => keystorePropertiesErrorEnable;
            set => SetProperty(ref keystorePropertiesErrorEnable, value);
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
                        //Extraction of MAC to identify the SAM-SE
                        SAM_SEProperties!.SAM_SEMac = ProgrammingStation.Split('\t')[1];
                    }
                    else
                    {
                        SAM_SEProperties!.SAM_SEMac = string.Empty;
                    }
                }
            }
        }

        public async Task RefreshStationProgrammationList()
        {
            try
            {
                RefreshList = false;
                KeyStorePropertiesErrorEnable = false;
                string prevru = ProgrammingStation;
                SAM_SEReaders.Clear();
                Mouse.OverrideCursor = Cursors.Wait;
                await SAM_SEDll.DetectSAM_SE();
                uint nb = SAM_SEDll.GetNumberSAM_SE();
                for (uint i = 0; i < nb; i++)
                {
                    SAM_SEDll.BlinkLedId(i, 0);

                    String version = SAM_SEDll.GetVersion(i);
                    String mac = SAM_SEDll.GetMac(i);
                    String type = SAM_SEDll.GetType(i);

                    SAM_SEReaders.Add(type + '\t' + mac + '\t' + version + '\t');
                }
                //If we find the old element which was selected before the refresh
                if (SAM_SEReaders.Contains(prevru))
                {
                    //We select it back and make the Led blink to notify the user
                    ProgrammingStation = prevru;
                    SAM_SEDll.BlinkLedId((uint)SAM_SEStationIndex, 1);
                }
                //If we don't find the old element, and if there is at least one programming station
                else if (SAM_SEReaders.Count != 0)
                {
                    //We select the first one of the list, and we make its Led blink
                    ProgrammingStation = SAM_SEReaders.First();
                    SAM_SEDll.BlinkLedId(0, 1);
                }
                //If there is no programming station, we put the string as empty
                else
                {
                    ProgrammingStation = string.Empty;
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
