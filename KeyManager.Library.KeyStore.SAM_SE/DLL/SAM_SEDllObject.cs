using System.Runtime.InteropServices;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL
{
    public class SAM_SEDllObject
    {
        //Pointer to the SAM-SE context to be given to functions working on a SAM-SE
        public readonly UIntPtr Context;
        //Object used to manage DLL errors
        public readonly SAM_SEDllErrorHandler ErrorHandler;
        //Object ID's, readable for a human
        public readonly string StringId;
        //Object ID's, numeric format
        public readonly uint Id;
        //Object ID's that is linked to this object
        public SAM_SEDllObject? LinkedObject;
        //Type of object
        private readonly SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType Type;
        //Enum for the metadatas functions
        //This enum comes from the SPSE_DLL
        //Any changes in those index will result in an undefined behaviour
        public enum SAM_SEMetadataIndex
        {
            META_BYTE_DES_MODE = 0,     /*<! Byte of the DESFire configuration indicating the mode */
            META_BOOL_DES_MSB,          /*<! Binary of the DESFire configuration indicating whether MSB first is active */
            META_BPTR_DES_AID,          /*<! Byte array of the DESFire configuration specifying the AID */
            META_BYTE_DES_KEYNB,        /*<! Byte of the DESFire configuration indicating the key number */
            META_BYTE_DES_FILENB,       /*<! Byte of the DESFire configuration indicating the file number */
            META_BYTE_DES_OFFSET,       /*<! Byte of the DESFire configuration indicating the offset */
            META_BYTE_DES_IDSIZE,       /*<! Byte of the DESFire configuration indicating the size of the ID */
            META_BYTE_DES_ENC,          /*<! Byte of the DESFire configuration indicating the encryption type */
            META_BYTE_DES_COMM,         /*<! Byte of the DESFire configuration indicating whether communication is encrypted */
            META_BOOL_DES_EV0,          /*<! Binary of the DESFire configuration indicating whether EV0 technology is active */
            META_BOOL_DES_EV1,          /*<! Binary of the DESFire configuration indicating whether EV1 technology is active */
            META_BOOL_DES_EV2,          /*<! Binary of the DESFire configuration indicating whether EV2 technology is active */
            META_BOOL_DES_AUTHEV2,      /*<! Binary of the DESFire configuration indicating whether EV2 authentication is active */
            META_BOOL_DES_EV3,          /*<! Binary of the DESFire configuration indicating whether EV3 technology is active */
            META_BOOL_DES_PROXCHECK,    /*<! Binary of the DESFire configuration indicating whether Proximity Check is active */
            META_BOOL_DES_JCOP,         /*<! Binary of the DESFire configuration indicating whether JCOP technology is active */
            META_BOOL_UID_ACTIVE,       /*<! Binary indicating whether the random UID option is active */
            META_BYTE_UID_KEYNB,        /*<! Byte of the random UID configuration indicating the key ID */
            META_BOOL_DIV_ACTIVE,       /*<! Binary indicating whether diversification is active */
            META_BOOL_DIV_INVAID,       /*<! Binary of the diversification configuration indicating whether the AID is inverted */
            META_BPTR_DIV_SYSID,        /*<! Byte array of the diversification configuration specifying the System Identifier */
            META_BOOL_DIV_INCKEYSI      /*<! Binary of the diversification configuration indicating whether the key number is included in the SI */
        }
        //Enum for the policies of object
        //This enum comes from the SPSE_DLL
        //Any changes in those index will result in an undefined behaviour
        enum SAM_SEPoliciesIndex
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

        public SAM_SEDllObject(UIntPtr ctx, string stringID, uint id, SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType type, SAM_SEDllErrorHandler errorHandler)
        {
            Context = ctx;
            StringId = stringID;
            Id = id;
            ErrorHandler = errorHandler;
            Type = type;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_uploadKey(UIntPtr currentSAMSE, byte[] key, uint keyLen, uint keyId);
        public int UploadKey(string key)
        {
            byte[] result = new byte[key.Length / 2];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(key.Substring(i * 2, 2), 16);
            }
            int ret = spse_uploadKey(Context, result, (uint)result.Length, Id);
            ret = ErrorHandler.HandlingError(ret);
            BlinkLed(1);
            return ret;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint spse_getUIDKeyLinked(uint keyID);
        public string? GetUidKeyLinked()
        {
            uint id = spse_getUIDKeyLinked(Id);
            BlinkLed(1);
            if (id != 0)
            {
                LinkedObject = SAM_SEDllCard.GetSAM_SEObject(id);
                if (LinkedObject != null)
                {
                    return LinkedObject.StringId;
                }
            }
            return null;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_blinkLed(UIntPtr currentSAM_SE, uint state);
        public int BlinkLed(uint state)
        {
            int ret = spse_blinkLed(Context, state);
            ret = ErrorHandler.HandlingError(ret);
            return ret;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern byte spse_getKeyPolicy(uint keyId, SAM_SEPoliciesIndex policy);
        public bool GetPolicyReadable()
        {
            return spse_getKeyPolicy(Id, SAM_SEPoliciesIndex.POLICY_ALLOW_READ) == 1;
        }

        public bool GetPolicyWritable()
        {
            return spse_getKeyPolicy(Id, SAM_SEPoliciesIndex.POLICY_ALLOW_WRITE) == 1;
        }

        public bool GetPolicyEncypher()
        {
            return spse_getKeyPolicy(Id, SAM_SEPoliciesIndex.POLICY_ALLOW_ENC) == 1;
        }

        public bool GetPolicyDecypher()
        {
            return spse_getKeyPolicy(Id, SAM_SEPoliciesIndex.POLICY_ALLOW_DEC) == 1;
        }        

        public bool GetPolicyAuthDes()
        {
            return spse_getKeyPolicy(Id, SAM_SEPoliciesIndex.POLICY_ALLOW_DES_AUTH) == 1;
        }

        public bool GetPolicySessionDesDump()
        {
            return spse_getKeyPolicy(Id, SAM_SEPoliciesIndex.POLICY_ALLOW_DES_DUMP) == 1;
        }

        public bool GetPolicyImpExport()
        {
            return spse_getKeyPolicy(Id, SAM_SEPoliciesIndex.POLICY_ALLOW_IMP_EXP) == 1;
        }

        public bool GetPolicyWrapable()
        {
            return spse_getKeyPolicy(Id, SAM_SEPoliciesIndex.POLICY_ALLOW_WRAP) == 1;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_getMetadataByte(UIntPtr context, uint keyId, SAM_SEMetadataIndex metadata);
        public byte GetMetadataByte(SAM_SEMetadataIndex metadata)
        {
            int ret = spse_getMetadataByte(Context, Id, metadata);
            if (ret < 0)
            {
                ErrorHandler.HandlingError(ret);
                BlinkLed(1);
            }
            return (byte)ret;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_getMetadataBool(UIntPtr context, uint keyId, SAM_SEMetadataIndex metadata);
        public bool GetMetadataBool(SAM_SEMetadataIndex metadata)
        {
            int ret = spse_getMetadataBool(Context, Id, metadata);
            if (ret < 0)
            {
                ErrorHandler.HandlingError(ret);
                BlinkLed(1);
            }
            return ret == 1;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spse_getMetadataBytePtr(UIntPtr context, uint keyId, SAM_SEMetadataIndex metadata, ref byte size);

        public byte[] GetMetadataBytePtr(SAM_SEMetadataIndex metadata)
        {
            byte size = 0;
            IntPtr ret = spse_getMetadataBytePtr(Context, Id, metadata, ref size);
            if (ret == IntPtr.Zero)
            {
                ErrorHandler.HandlingError((int)SAM_SEDllErrorHandler.SAM_SEError.DLL_SPSE_WRONG_ARGUMENTS);
                BlinkLed(1);
            }
            byte[] value = new byte[size];
            Marshal.Copy(ret, value, 0, size);
            return value;
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_setMetadataByte(UIntPtr context, uint keyId, SAM_SEMetadataIndex metadata, byte value);
        public void SetMetadataByte(SAM_SEMetadataIndex metadata, byte value)
        {
            if (ErrorHandler.HandlingError(spse_setMetadataByte(Context, Id, metadata, value)) != 0)
                BlinkLed(1);
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_setMetadataBool(UIntPtr context, uint keyId, SAM_SEMetadataIndex metadata, bool value);
        public void SetMetadataBool(SAM_SEMetadataIndex metadata, bool value)
        {
            if (ErrorHandler.HandlingError(spse_setMetadataBool(Context, Id, metadata, value)) != 0)
                BlinkLed(1);
        }

        [DllImport(SAM_SEDllProgrammingStation.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_setMetadataBytePtr(UIntPtr context, uint keyId, SAM_SEMetadataIndex metadata, byte[] value, int size);
        public void SetMetadataBytePtr(SAM_SEMetadataIndex metadata, byte[] value)
        {
            if (ErrorHandler.HandlingError(spse_setMetadataBytePtr(Context, Id, metadata, value, value.Length)) != 0)
                BlinkLed(1);
        }

        //Method to get the type of the object
        public SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType GetKeyType() => Type;

    }
}
