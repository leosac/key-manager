/*
** File Name: SAM_SEDllAuthenticate.cs
** Author: s_eva
** Creation date: October 2024
** Description: This file exposes methods from DLL which works with Reader object.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using System;
using System.Runtime.InteropServices;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL
{
    public class SAM_SEDllReader(UIntPtr ctx, string stringID, uint id, SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType type) : SAM_SEDllObject(ctx, stringID, id, type)
    {
        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_uploadKeyK(UIntPtr currentSAMSE, SAM_SEReaderKeyType keyType, byte[]? key, uint keyLen, bool newKeyActive, SAM_SEReaderKeyType newKeyType, byte[]? newKey, uint newKeyLen);
        public int UploadReaderKey(SAM_SEReaderKeyType keyType, string? key, bool newKeyActive, SAM_SEReaderKeyType newKeyType, string? newKey)
        {
            byte[]? keyByte;
            uint keyByteLen;
            if (key != null)
            {
                keyByte = new byte[key.Length / 2];

                for (int i = 0; i < keyByte.Length; i++)
                {
                    keyByte[i] = Convert.ToByte(key.Substring(i * 2, 2), 16);
                }
                keyByteLen = (uint)keyByte.Length;
            }
            else
            {
                keyByte = null;
                keyByteLen = 0;
            }

            byte[]? newKeyByte;
            uint newKeyByteLen;
            if (newKey != null)
            {
                newKeyByte = new byte[newKey.Length / 2];

                for (int i = 0; i < newKeyByte.Length; i++)
                {
                    newKeyByte[i] = Convert.ToByte(newKey.Substring(i * 2, 2), 16);
                }
                newKeyByteLen = (uint)newKeyByte.Length;
            }
            else
            {
                newKeyByte = null;
                newKeyByteLen = 0;
            }

            int ret = spse_uploadKeyK(Context, keyType, keyByte, keyByteLen, newKeyActive, newKeyType, newKeyByte, newKeyByteLen);
            ret = SAM_SEDllConstants.ErrorHandler.HandlingError(ret);
            BlinkLed(1);
            return ret;
        }

        public bool GetNewKeyActive()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_NEWKEYK_ACTIVE);
        }

        public byte GetCurrentKeyType()
        {
            return GetMetadataByte(SAM_SEMetadataIndex.META_BYTE_KEYK_TYPE);
        }

        public byte GetNewKeyType()
        {
            return GetMetadataByte(SAM_SEMetadataIndex.META_BYTE_NEWKEYK_TYPE);
        }

    }
}
