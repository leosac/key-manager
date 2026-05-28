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
            Path = GetProgrammingStationPath();
            SAM_SE = new(Context, Index);
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
