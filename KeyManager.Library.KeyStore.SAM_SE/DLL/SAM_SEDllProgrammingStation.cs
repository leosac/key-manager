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
        //String with MAC of SAM-SE
        public string Mac;
        //String with version of SAM-SE
        public string Version;
        //String with version of SAM-SE
        public string Path;

        //Pointer to the Programming Station context to give to functions
        public readonly UIntPtr Context;
        //Dictionnary to link the SAM-SE Class "DLL_SAM_SE_Card" and the id of its programming station
        public SAM_SEDllCard SAM_SE;

        private readonly uint Index;

        //Constructor will get all the size, those are constants in each session
        public SAM_SEDllProgrammingStation(uint index, UIntPtr ctx)
        {
            Context = ctx;
            Index = index;
            Mac = GetMac(Index);
            Version = GetVersion(Index);
            Path = GetProgrammingStationPath();
            SAM_SE = new(Context);
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_blinkLed(UIntPtr currentSAM_SE, uint state);
        public int BlinkLed(uint state)
        {
            int ret = spse_blinkLed(Context, state);
            ret = SAM_SEDllConstants.ErrorHandler.HandlingError(ret);
            return ret;
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void spse_freeSpse(UIntPtr currentSAM_SE);
        public void Deinit()
        {
            spse_freeSpse(Context);
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

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spse_getDevicePath(UIntPtr ctx);
        public string GetProgrammingStationPath()
        {
            //Getting pointer from DLL
            IntPtr ptr = spse_getDevicePath(Context);
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
    }
}
