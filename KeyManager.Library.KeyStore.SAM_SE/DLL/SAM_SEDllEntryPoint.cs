/*
** File Name: SAM_SEDllEntryPoint.cs
** Author: s_eva
** Creation date: May 2024
** Description: This file is the entry point of the DLL, it contains a list of SAM_SEDllProgrammingStation.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.Properties;
using System.Runtime.InteropServices;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL
{
    public class SAM_SEDllEntryPoint
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        //Version of DLL
        private string VersionDll { get; set; } = string.Empty;
        //Dictionnary to link the SAM-SE Class "DLL_SAM_SE_Card" and the id of its programming station
        private Dictionary<uint, SAM_SEDllProgrammingStation> ProgrammingStationList { get; } = [];
        //Index used, updated with SelectSAM_SE
        private int CurrentIndex = -1;

        public SAM_SEDllEntryPoint()
        {
            VersionDll = GetDllVersion();
            CheckVersionDll(VersionDll);
            log.Info(string.Format("Opening {0} ; Version : {1}", SAM_SEDllConstants.SPSEDllPath, VersionDll));
        }

        private void CheckVersionDll(string version)
        {
            uint number;
            //"SPSE_DLL vX.Y.Z" --> ["SPSE_DLL ","X.Y.Z"] --> ["X","Y","Z"]
            string[] versions = version.Split("v")[1].Split(".");
            if (uint.TryParse(versions[0], out number))
            {
                if (number != SAM_SEDllConstants.SPSEDll_EXPECTED_MAJOR)
                {
                    throw new KeyStoreException(Resources.SAM_SEDllVersionError);
                }
            }
            else
                throw new KeyStoreException(Resources.SAM_SEDllVersionErrorNotDigit);
            if (uint.TryParse(versions[1], out number))
            {
                if (number != SAM_SEDllConstants.SPSEDll_EXPECTED_MINOR)
                {
                    log.Warn(string.Format("{0} is not matching the expected version {1}.{2}.0. It may result with undefined behaviour", VersionDll, SAM_SEDllConstants.SPSEDll_EXPECTED_MAJOR, SAM_SEDllConstants.SPSEDll_EXPECTED_MINOR));
                }
            }
            else
                throw new KeyStoreException(Resources.SAM_SEDllVersionErrorNotDigit);
            if (uint.TryParse(versions[2], out number))
            {
                if (number != 0)
                {
                    log.Warn(string.Format("{0} is a development release. It may result with undefined behaviour", VersionDll));
                }
            }
            else
                throw new KeyStoreException(Resources.SAM_SEDllVersionErrorNotDigit);
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void spse_freeDll();
        public static void Deinit()
        {
            spse_freeDll();
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spse_getStrNameVersion();
        public static string GetDllVersion()
        {
            IntPtr temp = spse_getStrNameVersion();
            string? result = Marshal.PtrToStringAnsi(temp);
            if (result == null)
                return string.Empty;
            return result;
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_blinkLedId(uint index, uint state);
        public int BlinkLedId(uint index, uint state)
        {
            int ret = spse_blinkLedId(index, state);
            ret = SAM_SEDllConstants.ErrorHandler.HandlingError(ret);
            return ret;
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern byte spse_getNumberSpse();
        public byte GetNumberProgrammingStation()
        {
            return spse_getNumberSpse();
        }

        //Method to deselect a Programming Station
        public void DeselectProgrammingStation(uint index)
        {
            if (ProgrammingStationList.ContainsKey(index))
            {
                ProgrammingStationList[index].BlinkLed(0);
                ProgrammingStationList[index].Deinit();
                ProgrammingStationList.Remove(index);
            }
            CurrentIndex = -1;
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr spse_selectByPath(string? path);
        public void SelectProgrammingStation(string path)
        {
            //Selection of SAM-SE
            UIntPtr temp = spse_selectByPath(path);

            SAM_SEConnectionState hasSAM_SE = GetSAM_SEPresence(0);

            switch (hasSAM_SE)
            {
                default:
                case SAM_SEConnectionState.STATION_NOT_CONNECTED:
                    throw new KeyStoreException(Resources.SAM_SEErrorNoSPSE);               
                case SAM_SEConnectionState.STATION_CONNECTED_NO_SAMSE:
                    throw new KeyStoreException(Resources.SAM_SEMissing);          
                case SAM_SEConnectionState.STATION_CONNECTED_WITH_SAMSE:
                    //It's added to the SAM-SE list, and we update the selected SAM-SE index
                    ProgrammingStationList.Add(0, new SAM_SEDllProgrammingStation(0, temp));
                    CurrentIndex = 0;
                    break;
            }
        }

        public void DeselectProgrammingStation(string path)
        {
            foreach(var change in ProgrammingStationList)
            {
                if (change.Value.Path == path)
                {
                    DeselectProgrammingStation(change.Key);
                }
            }
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_detect();
        public async Task<int> DetectProgrammingStation()
        {
            int ret = await Task.Run(spse_detect);
            ret = SAM_SEDllConstants.ErrorHandler.HandlingError(ret);
            return ret;
        }
        public int BlockingDetectProgrammingStation()
        {
            int ret = spse_detect();
            ret = SAM_SEDllConstants.ErrorHandler.HandlingError(ret);
            return ret;
        }


        //Method to get the current selected programming station
        public SAM_SEDllProgrammingStation? GetCurrentProgrammingStation()
        {
            if (CurrentIndex == -1 || !ProgrammingStationList.ContainsKey((uint)CurrentIndex))
                return null;
            return ProgrammingStationList[(uint)CurrentIndex];
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spse_getInformation(uint index, SAM_SEInformations type);
        public string GetMac(uint index)
        {
            IntPtr ptr = spse_getInformation(index, SAM_SEInformations.INFORMATIONS_MAC);
            if (ptr == IntPtr.Zero)
            {
                return "0C000000";
            }
            byte[] macArray = new byte[SAM_SEDllConstants.SizeMac];
            Marshal.Copy(ptr, macArray, 0, SAM_SEDllConstants.SizeMac);
            string[] macStr = Array.ConvertAll(macArray, b => b.ToString("X2"));
            string mac = string.Join("", macStr);
            return mac;
        }

        public string GetVersion(uint index)
        {
            IntPtr ptr = spse_getInformation(index, SAM_SEInformations.INFORMATIONS_VERSION);
            if (ptr == IntPtr.Zero)
            {
                return "v0.0.0";
            }
            byte[] versionArray = new byte[SAM_SEDllConstants.SizeVersion];
            Marshal.Copy(ptr, versionArray, 0, SAM_SEDllConstants.SizeVersion);
            string[] versionStr = Array.ConvertAll(versionArray, b => b.ToString());
            versionStr[0] = versionStr[0].TrimStart('0');
            string version = "v" + string.Join(".", versionStr);
            return version;
        }
        
        public string GetPath(uint index)
        {
            //Getting pointer from DLL
            IntPtr ptr = spse_getInformation(index, SAM_SEInformations.INFORMATIONS_PATH);
            if (ptr == IntPtr.Zero)
            {
                return string.Empty;
            }
            //Conversion to string
            string? path = Marshal.PtrToStringAnsi(ptr);
            if (path == null)
            {
                return string.Empty;
            }
            return path;
        }

        public SAM_SEConnectionState GetSAM_SEPresence(uint index)
        {
            //Getting pointer from DLL
            IntPtr ptr = spse_getInformation(index, SAM_SEInformations.INFORMATIONS_HAS_SAMSE);
            if (ptr == IntPtr.Zero)
            {
                return SAM_SEConnectionState.STATION_NOT_CONNECTED;
            }
            //Conversion to string
            string? path = Marshal.PtrToStringAnsi(ptr);
            if (path == null)
            {
                return SAM_SEConnectionState.STATION_NOT_CONNECTED;
            }
            else if (path == "false")
            {
                return SAM_SEConnectionState.STATION_CONNECTED_NO_SAMSE;
            }
            else if (path == "true")
            {
                return SAM_SEConnectionState.STATION_CONNECTED_WITH_SAMSE;
            }
            return SAM_SEConnectionState.STATION_NOT_CONNECTED;
        }
    }
}
