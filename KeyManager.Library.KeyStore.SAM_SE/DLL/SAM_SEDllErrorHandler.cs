/*
** File Name: SAM_SEDllErrorHandler.cs
** Author: s_eva
** Creation date: January 2024
** Description: Thie file exposes method to handle errors from the DLL.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.Properties;
using System.Runtime.InteropServices;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL
{
    public class SAM_SEDllErrorHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        //Traduction dictionnary between DLL Error and the string in FR/EN
        private readonly Dictionary<SAM_SEError, string> ErrorTraduction = new()
        {
            { SAM_SEError.DLL_SPSE_ON_ERROR, Resources.SAM_SEErrorOnError },
            { SAM_SEError.DLL_SPSE_DONE, Resources.SAM_SEErrorDone },
            { SAM_SEError.DLL_SPSE_WRONG_PTR, Resources.SAM_SEErrorWrongPtr },
            { SAM_SEError.DLL_SPSE_DISCONNECTED, Resources.SAM_SEErrorProbablyDisconnected },
            { SAM_SEError.DLL_SPSE_ILLOGICAL_OPERATION, Resources.SAM_SEErrorIllogicalAction },
            { SAM_SEError.DLL_SPSE_WRONG_ARGUMENTS, Resources.SAM_SEErrorWrongArg },
            { SAM_SEError.DLL_SPSE_WRONG_SIZE, Resources.SAM_SEErrorWrongSize },
            { SAM_SEError.DLL_SPSE_WRONG_INDEX, Resources.SAM_SEErrorWrongIndex },
            { SAM_SEError.DLL_SPSE_WARN_CONF_VERS, Resources.SAM_SEErrorOldVersion },
            { SAM_SEError.DLL_SPSE_BAD_FILE, Resources.SAM_SEErrorBadConditions },
            { SAM_SEError.DLL_SPSE_NO_SAM, Resources.SAM_SEErrorNoSPSE },
            { SAM_SEError.DLL_SPSE_WRONG_PWD, Resources.SAM_SEErrorWrongPwd },
        };

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spse_getLastError();
        public int HandlingError(int error)
        {
            //First we log the error, if it's a real error
            if (error != (int)SAM_SEError.DLL_SPSE_DONE)
                log.Error(String.Format("Error {0}",error));
            //If it's a SAM-SE error, we need to call a specific function
            if (error == (int)SAM_SEError.DLL_SPSE_ON_ERROR)
            {
                IntPtr temp = spse_getLastError();
                string? result = Marshal.PtrToStringAnsi(temp);
                //So we log the new return
                log.Error(result);
                //And an exception is created with the error
                throw new KeyStoreException(result);
            }
            //Else, it's a programming station error, we know what is the error right away
            else
            {
                if ((SAM_SEError)error != SAM_SEError.DLL_SPSE_DONE)
                {
                    if (ErrorTraduction.ContainsKey((SAM_SEError)error))
                    {
                        //Error from the enum, so message is already good, we can just throw an exception
                        throw new KeyStoreException(ErrorTraduction[(SAM_SEError)error]);
                    }
                    else
                    {
                        //Unknown error, so we need to format the message with the error number, and a standard message
                        throw new KeyStoreException(String.Format("{0}{1}", Resources.SAM_SEUnknownError, error));
                    }
                }
                //Here we should only return if the error is "Done" ; so no error in fact
                return error;
            }
        }
    }
}
