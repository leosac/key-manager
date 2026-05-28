/*
** File Name: SAM_SEDllEntryPoint.cs
** Author: s_eva
** Creation date: May 2024
** Description: This file contains all the enums used in the DLL context.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using System.Runtime.InteropServices;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL
{
    //Enum to list the differents informations of SAM-SE
    //This enum comes from the DLL, do not modify without any knowledge of the DLL
    public enum SAM_SEInformations
    {
        INFORMATIONS_MAC = 0,       /*<! Mac of SAM-SE */
        INFORMATIONS_VERSION,       /*<! Version of SAM-SE */
        INFORMATIONS_PATH,          /*<! Device path of the station */
        INFORMATIONS_HAS_SAMSE,     /*<! Indicates whether the station has a SAM-SE */
        INFORMATIONS_VERSION_UT,    /*<! Version du contexte UT du SAM-SE */
        INFORMATIONS_VERSION_UGL,	/*<! Version du contexte UGL du SAM-SE */
    }

    //Enum to list the differents lock level of SAM-SE
    //This enum comes from the DLL, do not modify without any knowledge of the DLL
    public enum SAM_SELockLevel
    {
        LOCK_LVL_KEYS = 0,      /*<! Minimal lock, only secrets keys are locked (only for Synchronic software not KML) */
        LOCK_LVL_FILE,          /*<! Intermediate lock, keys and files are locked for Synchronic Hardware */
        LOCK_LVL_READONLY,      /*<! Advanced lock, keys and files are locked for Synchronic Hardware, configuration file in read only mode for Synchronic software */
        //Add enum above this comment ; LOCK_LVL_NB_MAX is used as the maximum value not assignable
        LOCK_LVL_NB_MAX,
    }

    //Enum of error values from DLL
    //This enum comes from the SPSE_DLL
    //Any changes in those index will result in an undefined behaviour
    public enum SAM_SEError : int
    {
        DLL_SPSE_ARCH_ON_ERROR = -11,          /* In cas of error on the Archive-SE layer */
        DLL_SPSE_ILLOGICAL_OPERATION = -10,    /* Operation on file is impossible */
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

    //Enum for the metadatas functions
    //This enum comes from the SPSE_DLL
    //Any changes in those index will result in an undefined behaviour
    public enum SAM_SEMetadataIndex
    {
        META_BYTE_DES_MODE = 0,             /*<! Byte of the DESFire configuration indicating the mode */
        META_BOOL_DES_MSB,                  /*<! Binary of the DESFire configuration indicating whether MSB first is active */
        META_BPTR_DES_AID,                  /*<! Byte array of the DESFire configuration specifying the AID */
        META_BYTE_DES_KEYNB,                /*<! Byte of the DESFire configuration indicating the key number */
        META_BYTE_DES_FILENB,               /*<! Byte of the DESFire configuration indicating the file number */
        META_BYTE_DES_OFFSET,               /*<! Byte of the DESFire configuration indicating the offset */
        META_BYTE_DES_IDSIZE,               /*<! Byte of the DESFire configuration indicating the size of the ID */
        META_BYTE_DES_ENC,                  /*<! Byte of the DESFire configuration indicating the encryption type */
        META_BYTE_DES_COMM,                 /*<! Byte of the DESFire configuration indicating whether communication is encrypted */
        META_BOOL_DES_EV0,                  /*<! Binary of the DESFire configuration indicating whether EV0 technology is active */
        META_BOOL_DES_EV1,                  /*<! Binary of the DESFire configuration indicating whether EV1 technology is active */
        META_BOOL_DES_EV2,                  /*<! Binary of the DESFire configuration indicating whether EV2 technology is active */
        META_BOOL_DES_AUTHEV2,              /*<! Binary of the DESFire configuration indicating whether EV2 authentication is active */
        META_BOOL_DES_EV3,                  /*<! Binary of the DESFire configuration indicating whether EV3 technology is active */
        META_BOOL_DES_PROXCHECK,            /*<! Binary of the DESFire configuration indicating whether Proximity Check is active */
        META_BOOL_DES_JCOP,                 /*<! Binary of the DESFire configuration indicating whether JCOP EV1 technology is active */
        META_BOOL_UID_ACTIVE,               /*<! Binary indicating whether the random UID option is active */
        META_BYTE_UID_KEYNB,                /*<! Byte of the random UID configuration indicating the key ID */
        META_BOOL_DIV_ACTIVE,               /*<! Binary indicating whether diversification is active */
        META_BOOL_DIV_INVAID,               /*<! Binary of the diversification configuration indicating whether the AID is inverted */
        META_BPTR_DIV_SYSID,                /*<! Byte array of the diversification configuration specifying the System Identifier */
        META_BOOL_DIV_INCKEYSI,             /*<! Binary of the diversification configuration indicating whether the key number is included in the SI */
        META_BOOL_DES_JCOP_EV3,		        /*<! Binary of the DESFire configuration indicating whether JCOP EV3 technology is active */
        META_BYTE_KEYK_TYPE,                /*<! Byte of the reader current key, indicating if it's personalized or synchronic */
        META_BOOL_NEWKEYK_ACTIVE,           /*<! Binary of the reader key indicating whether the new key is active or not */
        META_BYTE_NEWKEYK_TYPE,		        /*<! Byte of the reader new key, indicating if it's personalized or synchronic */
    }

    //Enum for the policies of object
    //This enum comes from the SPSE_DLL
    //Any changes in those index will result in an undefined behaviour
    public enum SAM_SEReaderKeyType
    {
        KEY_READER_PERSONALIZED = 0,
        KEY_READER_SYNCHRONIC = 1,
    }

    //Enum for the policies of object
    //This enum comes from the SPSE_DLL
    //Any changes in those index will result in an undefined behaviour
    public enum SAM_SEPoliciesIndex
    {
        POLICY_ALLOW_READ = 0,      /*<! Allows reading of the object */
        POLICY_ALLOW_WRITE,         /*<! Allows writing to the object */
        POLICY_ALLOW_ENC,           /*<! Allows encryption */
        POLICY_ALLOW_DEC,           /*<! Allows decryption */
        POLICY_ALLOW_DES_AUTH,      /*<! Indicates that the object can be used for DESFire authentication */
        POLICY_ALLOW_DES_DUMP,      /*<! Indicates that the DESFire session key can be provided to the host */
        POLICY_ALLOW_IMP_EXP,       /*<! Indicates that the object can be imported or exported */
        POLICY_ALLOW_WRAP           /*<! Allows key wrapping (by master key) */
    }

    public enum SAM_SEConnectionState
    {
        STATION_NOT_CONNECTED = 0,
        STATION_CONNECTED_NO_SAMSE,
        STATION_CONNECTED_WITH_SAMSE,
    }

    public enum SAM_SECertificatesType
    {
        CERT_TYPE_CRT_RCA = 0,      /*<! Le fichier de types Racine d'autorité de certification */
        CERT_TYPE_CRT_CA,           /*<! Les fichiers de types autorités de certification */
        CERT_TYPE_CRT_TLS,          /*<! Le certificat destiné à la personnalisation du Module TLS */
        CERT_TYPE_KEY_TLS,          /*<! La clé privée destiné à la personnalisation du Module TLS */
        CERT_TYPE_CRT_RADIUS,       /*<! Le certificat destiné à la personnalisation du Module Radius */
        CERT_TYPE_KEY_RADIUS,       /*<! La clé privée destiné à la personnalisation du Module Radius */
        CERT_TYPE_CRT_WEB,          /*<! Le certificat destiné à la personnalisation du Module WEB */
        CERT_TYPE_KEY_WEB,          /*<! La clé privée destiné à la personnalisation du Module WEB */
        CERT_TYPE_CRT_JANUS,        /*<! Le certificat destiné à la personnalisation du Module Janus */
        CERT_TYPE_KEY_JANUS,        /*<! La clé privée destiné à la personnalisation du Module Janus */
        CERT_TYPE_NB				/*<! Le nombre de type de fichiers, doit rester la dernière valeur */
    }

    public enum SAM_SEConfigurationArchiveIni
    {
        CONF_INI_CU_NAME = 0,       /*<! Nom de l'UGL */
        CONF_INI_CU_ID,             /*<! Numéro de dossier UGL */
        CONF_INI_CU_IP,             /*<! Adresse IP UGL */
        CONF_INI_CU_MASK,           /*<! Masque de sous réseau UGL */
        CONF_INI_CU_GATEWAY,        /*<! Passerelle UGL */
        CONF_INI_ACS_IP,            /*<! Adresse IP Serveur Controle d'Accès */
        CONF_INI_PORT_FDE,          /*<! Port ServeurUG_TLS */
        CONF_INI_PORT_TRANS,        /*<! Port ServeurTransfertTLS */
        CONF_INI_RADIUS_IDENTITY,   /*<! Indentité du certificat pour le certificvat Radius */
        CONF_INI_NB                 /*<! Le nombre d'élements éditable de la configuration UGL */
    }

    public enum SAM_SEMaxSAN
    {
        SAN_MAX_NUM = 16
    }

    public class SAM_SEDllConstants
    {
        public static byte SizeMac { get; private set; } = GetMacSize();
        public static byte SizeVersion { get; private set; } = GetVersionSize();
        public static SAM_SEDllErrorHandler ErrorHandler { get; private set; } = new();
        //Path of DLL, we don't need a specific path, the software will find it anyway
        public const string SPSEDllPath = "SPSE_DLL.dll";
        //Expected MAJOR of the DLL version
        public const byte SPSEDll_EXPECTED_MAJOR = 1;
        //Expected MINOR of the DLL version
        public const byte SPSEDll_EXPECTED_MINOR = 5;

        [DllImport(SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern byte spse_getConstSize(SAM_SEInformations type);

        private static byte GetMacSize()
        {
            return spse_getConstSize(SAM_SEInformations.INFORMATIONS_MAC);
        }

        private static byte GetVersionSize()
        {
            return spse_getConstSize(SAM_SEInformations.INFORMATIONS_VERSION);
        }
    }
}
