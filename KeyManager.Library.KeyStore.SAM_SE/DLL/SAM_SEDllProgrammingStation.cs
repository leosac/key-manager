/*
** File Name: SAM_SEDllProgrammingStation.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file is the entry point of the DLL, it contains a list of SAM_SEDllCard.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using System.Runtime.InteropServices;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL
{
    public class SAM_SEDllProgrammingStation
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        //Size of Mac
        private byte SizeMac { get; set; } = 0;
        //Size of version
        private byte SizeVersion { get; set; } = 0;
        //Size of type
        private byte SizeType { get; set; } = 0;
        //Index used, updated with SelectSAM_SE
        private int CurrentIndex = -1;
        //Path of DLL, we don't need a specific path, the software will find it anyway
        public const string SPSEDllPath = "SPSE_DLL.dll";
        //Dictionnary to link the SAM-SE Class "DLL_SAM_SE_Card" and the id of its programming station
        public Dictionary<uint, SAM_SEDllCard> SAM_SEList = [];
        //Class to handle SAM-SE errors
        public SAM_SEDllErrorHandler ErrorHandler { get; set; } = new();

        //Enum to list the differents informations of SAM-SE
        //This enum comes from the DLL, do not modify without any knowledge of the DLL
        public enum SAM_SEInformations
        {
            INFORMATIONS_MAC = 0,   /*<! Mac of SAM-SE */
            INFORMATIONS_VERSION,   /*<! Version of SAM-SE */
        }

        //Constructor will get all the size, those are constants in each session
        public SAM_SEDllProgrammingStation()
        {
            log.Info(GetDllVersion());
            SizeMac = GetMacSize();
            SizeVersion = GetVersionSize();
            SizeType = GetTypeSize();
        }
        //Method to get the current selected SAM-SE
        public SAM_SEDllCard? GetCurrentSAM_SE()
        {
            if (CurrentIndex == -1 || !SAM_SEList.ContainsKey((uint)CurrentIndex))
                return null;
            return SAM_SEList[(uint)CurrentIndex];
        }

        [DllImport(SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_detect();
        public async Task<int> DetectSAM_SE()
        {
            int ret = await Task.Run(spse_detect);
            //Specific case where no SAM-SE connected is not generating a real error in KML
            if (ret != (int)SAM_SEDllErrorHandler.SAM_SEError.DLL_SPSE_NO_SAM)
                ret = ErrorHandler.HandlingError(ret);
            return ret;
        }
        public int DetectSAM_SEBlocking()
        {
            int ret = spse_detect();
            //Specific case where no SAM-SE connected is not generating a real error in KML
            if (ret != (int)SAM_SEDllErrorHandler.SAM_SEError.DLL_SPSE_NO_SAM)
                ret = ErrorHandler.HandlingError(ret);
            return ret;
        }

        [DllImport(SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr spse_select(uint index);
        public void SelectSAM_SE(uint index)
        {
            //Selection of SAM-SE
            UIntPtr temp = spse_select(index);

            //If we get a real return
            if (temp != UIntPtr.Zero)
            { 
                //It's added to the SAM-SE list, and we update the selected SAM-SE index
                SAM_SEList.Add(index, new SAM_SEDllCard(temp, GetMac(index), GetType(index), GetVersion(index), ErrorHandler));
                CurrentIndex = (int)index;
            }
            else
            {
                throw new KeyStoreException(Properties.Resources.ProgrammingStationMissing);
            }
        }
        //Method to deselect a SAM-SE
        public void DeselectSAM_SE(uint index)
        {
            if (SAM_SEList.ContainsKey(index))
            {
                SAM_SEList[index].BlinkLed(0);
                SAM_SEList[index].Deinit();
                SAM_SEList.Remove(index);
            }
            CurrentIndex = -1;
        }


        [DllImport(SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void spse_freeDll();
        public static void Deinit()
        {
            spse_freeDll();
        }

        [DllImport(SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern byte spse_getNumberSamSe();
        public byte GetNumberSAM_SE()
        {
            return spse_getNumberSamSe();
        }

        [DllImport(SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern byte spse_getConstSize(SAM_SEInformations type);
        public static byte GetMacSize()
        {
            return spse_getConstSize(SAM_SEInformations.INFORMATIONS_MAC);
        }

        public static byte GetVersionSize()
        {
            return spse_getConstSize(SAM_SEInformations.INFORMATIONS_VERSION);
        }

        public static byte GetTypeSize()
        {
            return 3;   //Type = SE, donc le pointeur sera de 3 : 'S''E''\0'
        }

        [DllImport(SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spse_getInformation(uint index, SAM_SEInformations type);
        public string GetType(uint index)
        {
            return "SE";
        }

        public string GetMac(uint index)
        {
            IntPtr ptr = spse_getInformation(index, SAM_SEInformations.INFORMATIONS_MAC);
            if (ptr == IntPtr.Zero)
            {
                return "0C000000";
            }
            byte[] macArray = new byte[SizeMac];
            Marshal.Copy(ptr, macArray, 0, SizeMac);
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
            byte[] versionArray = new byte[SizeVersion];
            Marshal.Copy(ptr, versionArray, 0, SizeVersion);
            string[] versionStr = Array.ConvertAll(versionArray, b => b.ToString());
            versionStr[0] = versionStr[0].TrimStart('0');
            string version = "v" + string.Join(".", versionStr);
            return version;
        }

        [DllImport(SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_blinkLedId(uint index, uint state);
        public int BlinkLedId(uint index, uint state)
        {
            int ret = spse_blinkLedId(index, state);
            ret = ErrorHandler.HandlingError(ret);
            return ret;
        }

        [DllImport(SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spse_getStrNameVersion();
        public static string? GetDllVersion()
        {
            IntPtr temp = spse_getStrNameVersion();
            string? result = Marshal.PtrToStringAnsi(temp);
            return result;
        }

        //Method used to find a SAM-SE with its MAC
        public int FindSAM_SEByMac(string mac)
        {
            byte nb = GetNumberSAM_SE();
            for (byte i = 0; i < nb; i++)
            {
                string macLu = GetMac(i);
                if (macLu == mac)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
