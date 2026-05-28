/*
** File Name: SAM_SEDllCertificates.cs
** Author: s_eva
** Creation date: September 2024
** Description: This file exposes methods from DLL which are used to interact with certificates
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using System.Runtime.InteropServices;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL
{
    public class SAM_SEDllCertificates
    {
        //Pointer to the SAM-SE context to be given to functions working on a SAM-SE
        public readonly UIntPtr Context;

        public SAM_SEDllCertificates(UIntPtr ctx)
        {
            Context = ctx;
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_initArch(UIntPtr ctx);
        public void InitArchive()
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_initArch(Context));
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_updateArch(UIntPtr ctx);
        public void UpdateArchive()
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_updateArch(Context));
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_isFileInstalled(UIntPtr ctx, SAM_SECertificatesType type);
        public uint IsFileInstalled(SAM_SECertificatesType type)
        {
            int ret = spse_isFileInstalled(Context, type);
            if (ret < 0)
            {
                SAM_SEDllConstants.ErrorHandler.HandlingError(ret);
            }
            return (uint)ret;
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spse_getCertCommonName(UIntPtr ctx, SAM_SECertificatesType type, int idCAInt);
        public string? GetCertificateCommonName(SAM_SECertificatesType type, uint idCAInt)
        {
            IntPtr ret = spse_getCertCommonName(Context, type, (int)idCAInt);
            if (ret == IntPtr.Zero)
            {
                return String.Empty;
            }
            return Marshal.PtrToStringAnsi(ret);
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spse_getFileName(UIntPtr ctx, SAM_SECertificatesType type, int idCAInt);
        public string? GetFileName(SAM_SECertificatesType type, uint idCAInt)
        {
            IntPtr ret = spse_getFileName(Context, type, (int)idCAInt);
            if (ret == IntPtr.Zero)
            {
                return String.Empty;
            }
            return Marshal.PtrToStringAnsi(ret);
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_addArchFile(UIntPtr ctx, string? path, SAM_SECertificatesType type, string? password);
        public void AddArchiveFile(SAM_SECertificatesType type, string path, string? password)
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_addArchFile(Context, path, type, password));
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_extractArchFile(UIntPtr ctx, SAM_SECertificatesType type, int idCAInt, string path);
        public string? ExtractArchiveFile(SAM_SECertificatesType type, string? path, uint idCAInt)
        {
            if (path == null)
                return string.Empty;
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_extractArchFile(Context, type, (int)idCAInt, path));
            return GetFileName(type, idCAInt);
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_deleteArchFile(UIntPtr ctx, SAM_SECertificatesType type, int idCAInt);
        public void DeleteArchiveFile(SAM_SECertificatesType type, uint idCAInt)
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_deleteArchFile(Context, type, (int)idCAInt));
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_genCsr(UIntPtr ctx, SAM_SECertificatesType type, string path, string?[] san, uint nbSan);
        public void GenerationCSR(SAM_SECertificatesType type, string? path, string?[] san, uint nbSan)
        {
            if (path == null)
                return;
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_genCsr(Context, type, path, san, nbSan));
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_setValueArchIniStr(UIntPtr ctx, SAM_SEConfigurationArchiveIni type, string? val);

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_setValueArchIniDec(UIntPtr ctx, SAM_SEConfigurationArchiveIni type, uint val);

        public void SetCUNameArchiveIni(string? val)
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_setValueArchIniStr(Context, SAM_SEConfigurationArchiveIni.CONF_INI_CU_NAME, val));
        }

        public void SetCUIdArchiveIni(uint val)
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_setValueArchIniDec(Context, SAM_SEConfigurationArchiveIni.CONF_INI_CU_ID, val));
        }

        public void SetCUIpArchiveIni(string? val)
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_setValueArchIniStr(Context, SAM_SEConfigurationArchiveIni.CONF_INI_CU_IP, val));
        }

        public void SetCUMaskArchiveIni(string? val)
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_setValueArchIniStr(Context, SAM_SEConfigurationArchiveIni.CONF_INI_CU_MASK, val));
        }

        public void SetCUGatewayArchiveIni(string? val)
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_setValueArchIniStr(Context, SAM_SEConfigurationArchiveIni.CONF_INI_CU_GATEWAY, val));
        }

        public void SetAccessControlIpArchiveIni(string? val)
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_setValueArchIniStr(Context, SAM_SEConfigurationArchiveIni.CONF_INI_ACS_IP, val));
        }

        public void SetCUServerPortArchiveIni(uint val)
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_setValueArchIniDec(Context, SAM_SEConfigurationArchiveIni.CONF_INI_PORT_FDE, val));
        }

        public void SetTLSServerPortArchiveIni(uint val)
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_setValueArchIniDec(Context, SAM_SEConfigurationArchiveIni.CONF_INI_PORT_TRANS, val));
        }

        public void SetRadiusIdentityArchiveIni(string? val)
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_setValueArchIniStr(Context, SAM_SEConfigurationArchiveIni.CONF_INI_RADIUS_IDENTITY, val));
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spse_getValueArchIniStr(UIntPtr ctx, SAM_SEConfigurationArchiveIni type, ref uint sizeVal);

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_getValueArchIniDec(UIntPtr ctx, SAM_SEConfigurationArchiveIni type);
        public string? GetCUNameArchiveIni()
        {
            uint sizeVal = 0;
            IntPtr ret = spse_getValueArchIniStr(Context, SAM_SEConfigurationArchiveIni.CONF_INI_CU_NAME, ref sizeVal);
            if (ret == IntPtr.Zero)
            {
                return String.Empty;
            }
            return Marshal.PtrToStringAnsi(ret);
        }

        public uint GetCUIdArchiveIni()
        {
            int ret = spse_getValueArchIniDec(Context, SAM_SEConfigurationArchiveIni.CONF_INI_CU_ID);
            if(ret<0)
            {
                SAM_SEDllConstants.ErrorHandler.HandlingError(ret);
            }
            return (uint)ret;
        }

        public string? GetCUIpArchiveIni()
        {
            uint sizeVal = 0;
            IntPtr ret = spse_getValueArchIniStr(Context, SAM_SEConfigurationArchiveIni.CONF_INI_CU_IP, ref sizeVal);
            if (ret == IntPtr.Zero)
            {
                return String.Empty;
            }
            return Marshal.PtrToStringAnsi(ret);
        }

        public string? GetCUMaskArchiveIni()
        {
            uint sizeVal = 0;
            IntPtr ret = spse_getValueArchIniStr(Context, SAM_SEConfigurationArchiveIni.CONF_INI_CU_MASK, ref sizeVal);
            if (ret == IntPtr.Zero)
            {
                return String.Empty;
            }
            return Marshal.PtrToStringAnsi(ret);
        }

        public string? GetCUGatewayArchiveIni()
        {
            uint sizeVal = 0;
            IntPtr ret = spse_getValueArchIniStr(Context, SAM_SEConfigurationArchiveIni.CONF_INI_CU_GATEWAY, ref sizeVal);
            if (ret == IntPtr.Zero)
            {
                return String.Empty;
            }
            return Marshal.PtrToStringAnsi(ret);
        }

        public string? GetAccessControlIpArchiveIni()
        {
            uint sizeVal = 0;
            IntPtr ret = spse_getValueArchIniStr(Context, SAM_SEConfigurationArchiveIni.CONF_INI_ACS_IP, ref sizeVal);
            if (ret == IntPtr.Zero)
            {
                return String.Empty;
            }
            return Marshal.PtrToStringAnsi(ret);
        }

        public uint GetCUServerPortArchiveIni()
        {
            int ret = spse_getValueArchIniDec(Context, SAM_SEConfigurationArchiveIni.CONF_INI_PORT_FDE);
            if (ret < 0)
            {
                SAM_SEDllConstants.ErrorHandler.HandlingError(ret);
            }
            return (uint)ret;
        }

        public uint GetTLSServerPortArchiveIni()
        {
            int ret = spse_getValueArchIniDec(Context, SAM_SEConfigurationArchiveIni.CONF_INI_PORT_TRANS);
            if (ret < 0)
            {
                SAM_SEDllConstants.ErrorHandler.HandlingError(ret);
            }
            return (uint)ret;
        }

        public string? GetRadiusIdentityArchiveIni()
        {
            uint sizeVal = 0;
            IntPtr ret = spse_getValueArchIniStr(Context, SAM_SEConfigurationArchiveIni.CONF_INI_RADIUS_IDENTITY, ref sizeVal);
            if (ret == IntPtr.Zero)
            {
                return String.Empty;
            }
            return Marshal.PtrToStringAnsi(ret);
        }
    }
}
