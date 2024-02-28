using System.Runtime.InteropServices;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL
{
    public class SAM_SEDllAuthenticate(UIntPtr ctx, string stringID, uint id, SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType type, SAM_SEDllErrorHandler errorHandler) : SAM_SEDllObject(ctx, stringID, id, type, errorHandler)
    {
        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_changePassword(UIntPtr currentSAMSE, string newPassword, uint newPswdLen);
        public int ChangePassword(string newPassword, uint newPswdLen)
        {
            int ret = spse_changePassword(Context, newPassword, newPswdLen);
            ret = ErrorHandler.HandlingError(ret);
            BlinkLed(1);
            return ret;
        }
    }
}
