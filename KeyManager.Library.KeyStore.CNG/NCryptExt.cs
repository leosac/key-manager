using System.Runtime.InteropServices;
using Vanara.PInvoke;

namespace Leosac.KeyManager.Library.KeyStore.CNG
{
    [PInvokeData("ncrypt.h", MSDNShortId = "NS:ncrypt.NCryptKeyName")]
    [StructLayout(LayoutKind.Sequential)]
    public struct NCryptKeyName
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string pszName;

        [MarshalAs(UnmanagedType.LPWStr)]
        internal string pszAlgId;

        internal int dwLegacyKeySpec;

        internal int dwFlags;
    }
}
