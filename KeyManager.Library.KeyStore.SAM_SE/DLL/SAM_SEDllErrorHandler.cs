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

        //Enum of error values from DLL
        //This enum comes from the SPSE_DLL
        //Any changes in those index will result in an undefined behaviour
        public enum SAM_SEError : int
        {
            DLL_SPSE_DISCONNECTED = -9,            /* SAM-SE seems disconnected */
            DLL_SPSE_WRONG_PTR = -8,               /* Incorrect pointer provided as argument */
            DLL_SPSE_WRONG_ARGUMENTS = -7,         /* Incorrect arguments */
            DLL_SPSE_WRONG_SIZE = -6,              /* Incorrect size */
            DLL_SPSE_WRONG_INDEX = -5,             /* Incorrect index */
            DLL_SPSE_BAD_FILE = -4,                /* Incorrect file */
            DLL_SPSE_NO_SAM = -3,                  /* No SAM-SE found */
            DLL_SPSE_WRONG_PWD = -2,               /* Incorrect password */
            DLL_SPSE_ON_ERROR = -1,                /* In case of error on the SAM-SE layer */
            DLL_SPSE_DONE = 0,                     /* Successfully completed */
            DLL_SPSE_WARN_CONF_VERS = 1            /* Warning! Configuration file version not up to date */
        }

        //Traduction dictionnary between DLL Error and the string in FR/EN
        private readonly Dictionary<SAM_SEError, string> ErrorTraduction = new()
        {
            { SAM_SEError.DLL_SPSE_ON_ERROR, Resources.SAM_SEErrorOnError },
            { SAM_SEError.DLL_SPSE_DONE, Resources.SAM_SEErrorDone },
            { SAM_SEError.DLL_SPSE_WRONG_PTR, Resources.SAM_SEErrorWrongPtr },
            { SAM_SEError.DLL_SPSE_DISCONNECTED, Resources.SAM_SEErrorProbablyDisconnected },
            { SAM_SEError.DLL_SPSE_WRONG_ARGUMENTS, Resources.SAM_SEErrorWrongArg },
            { SAM_SEError.DLL_SPSE_WRONG_SIZE, Resources.SAM_SEErrorWrongSize },
            { SAM_SEError.DLL_SPSE_WRONG_INDEX, Resources.SAM_SEErrorWrongIndex },
            { SAM_SEError.DLL_SPSE_WARN_CONF_VERS, Resources.SAM_SEErrorOldVersion },
            { SAM_SEError.DLL_SPSE_BAD_FILE, Resources.SAM_SEErrorBadConditions },
            { SAM_SEError.DLL_SPSE_NO_SAM, Resources.SAM_SEErrorNoSAM_SE },
            { SAM_SEError.DLL_SPSE_WRONG_PWD, Resources.SAM_SEErrorWrongPwd },
        };

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spse_getLastError();
        public int HandlingError(int error)
        {
            //First we log the error
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
