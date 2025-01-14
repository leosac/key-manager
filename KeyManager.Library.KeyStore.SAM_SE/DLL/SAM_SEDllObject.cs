/*
** File Name: SAM_SEDllObject.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file exposes methods from DLL which are generic to all objects.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using System.Runtime.InteropServices;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL
{
    public class SAM_SEDllObject
    {
        //Pointer to the SAM-SE context to be given to functions working on a SAM-SE
        public readonly UIntPtr Context;
        //Object ID's, readable for a human
        public readonly string StringId;
        //Object ID's, numeric format
        public readonly uint Id;
        //Object ID's that is linked to this object
        public SAM_SEDllObject? LinkedObject;
        //Type of object
        private readonly SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType Type;

        public SAM_SEDllObject(UIntPtr ctx, string stringID, uint id, SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType type)
        {
            Context = ctx;
            StringId = stringID;
            Id = id;
            Type = type;
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_uploadKey(UIntPtr currentSAMSE, byte[] key, uint keyLen, uint keyId);
        public int UploadKey(string key)
        {
            byte[] result = new byte[key.Length / 2];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(key.Substring(i * 2, 2), 16);
            }
            int ret = spse_uploadKey(Context, result, (uint)result.Length, Id);
            ret = SAM_SEDllConstants.ErrorHandler.HandlingError(ret);
            BlinkLed(1);
            return ret;
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
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

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_blinkLed(UIntPtr currentSAM_SE, uint state);
        public int BlinkLed(uint state)
        {
            int ret = spse_blinkLed(Context, state);
            ret = SAM_SEDllConstants.ErrorHandler.HandlingError(ret);
            return ret;
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
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

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_getMetadataByte(UIntPtr context, uint keyId, SAM_SEMetadataIndex metadata);
        public byte GetMetadataByte(SAM_SEMetadataIndex metadata)
        {
            int ret = spse_getMetadataByte(Context, Id, metadata);
            if (ret < 0)
            {
                SAM_SEDllConstants.ErrorHandler.HandlingError(ret);
                BlinkLed(1);
            }
            return (byte)ret;
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_getMetadataBool(UIntPtr context, uint keyId, SAM_SEMetadataIndex metadata);
        public bool GetMetadataBool(SAM_SEMetadataIndex metadata)
        {
            int ret = spse_getMetadataBool(Context, Id, metadata);
            if (ret < 0)
            {
                SAM_SEDllConstants.ErrorHandler.HandlingError(ret);
                BlinkLed(1);
            }
            return ret == 1;
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spse_getMetadataBytePtr(UIntPtr context, uint keyId, SAM_SEMetadataIndex metadata, ref byte size);
        public byte[] GetMetadataBytePtr(SAM_SEMetadataIndex metadata)
        {
            byte size = 0;
            IntPtr ret = spse_getMetadataBytePtr(Context, Id, metadata, ref size);
            if (ret == IntPtr.Zero)
            {
                SAM_SEDllConstants.ErrorHandler.HandlingError((int)SAM_SEError.DLL_SPSE_WRONG_ARGUMENTS);
                BlinkLed(1);
            }
            byte[] value = new byte[size];
            Marshal.Copy(ret, value, 0, size);
            return value;
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_setMetadataByte(UIntPtr context, uint keyId, SAM_SEMetadataIndex metadata, byte value);
        public void SetMetadataByte(SAM_SEMetadataIndex metadata, byte value)
        {
            if (SAM_SEDllConstants.ErrorHandler.HandlingError(spse_setMetadataByte(Context, Id, metadata, value)) != 0)
                BlinkLed(1);
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_setMetadataBool(UIntPtr context, uint keyId, SAM_SEMetadataIndex metadata, bool value);
        public void SetMetadataBool(SAM_SEMetadataIndex metadata, bool value)
        {
            if (SAM_SEDllConstants.ErrorHandler.HandlingError(spse_setMetadataBool(Context, Id, metadata, value)) != 0)
                BlinkLed(1);
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_setMetadataBytePtr(UIntPtr context, uint keyId, SAM_SEMetadataIndex metadata, byte[] value, int size);
        public void SetMetadataBytePtr(SAM_SEMetadataIndex metadata, byte[] value)
        {
            if (SAM_SEDllConstants.ErrorHandler.HandlingError(spse_setMetadataBytePtr(Context, Id, metadata, value, value.Length)) != 0)
                BlinkLed(1);
        }

        //Method to get the type of the object
        public SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType GetKeyType() => Type;

    }
}
