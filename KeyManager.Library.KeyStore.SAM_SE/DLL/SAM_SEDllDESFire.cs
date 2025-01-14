/*
** File Name: SAM_SEDllDESFire.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file exposes methods from DLL which works with DESFire object.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using System.Runtime.InteropServices;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL
{
    public class SAM_SEDllDESFire(UIntPtr ctx, string stringId, uint id, SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType type) : SAM_SEDllObject(ctx, stringId, id, type)
    {
        public SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireMode GetReadingMode()
        {
            return (SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireMode)GetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_MODE);
        }

        public bool GetMsb()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_MSB);
        }

        public byte[] GetAid()
        {
            return GetMetadataBytePtr(SAM_SEMetadataIndex.META_BPTR_DES_AID);
        }

        public byte GetKeyNumber()
        {
            return GetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_KEYNB);
        }

        public byte GetFileNumber()
        {
            return GetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_FILENB);
        }

        public byte GetOffset()
        {
            return GetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_OFFSET);
        }

        public byte GetSize()
        {
            return GetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_IDSIZE);
        }

        public SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireEncryptMode GetEncryption()
        {
            return (SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireEncryptMode)GetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_ENC);
        }

        public SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireCommunicationMode GetCommunication()
        {
            return (SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireCommunicationMode)GetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_COMM);
        }

        public bool GetEv0()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_EV0);
        }

        public bool GetEv1()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_EV1);
        }

        public bool GetEv2()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_EV2);
        }

        public bool GetAuthEv2()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_AUTHEV2);
        }

        public bool GetEv3()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_EV3);
        }

        public bool GetProximityCheck()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_PROXCHECK);
        }

        public bool GetJcop()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_JCOP);
        }

        public bool GetJcopEv3()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_JCOP_EV3);
        }

        public bool GetDivEnable()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DIV_ACTIVE);
        }

        public bool GetDivAidInv()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DIV_INVAID);
        }

        public byte[] GetDivSi()
        {
            return GetMetadataBytePtr(SAM_SEMetadataIndex.META_BPTR_DIV_SYSID);
        }

        public bool GetDivKeyEnable()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DIV_INCKEYSI);
        }

        public void SetReadingMode(SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireMode value)
        {
            SetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_MODE, (byte)value);
        }

        public void SetMsb(bool value)
        {
            SetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_MSB, value);
        }

        public void SetAid(byte[] AID)
        {
            SetMetadataBytePtr(SAM_SEMetadataIndex.META_BPTR_DES_AID, AID);
        }

        public void SetKeyNumber(byte value)
        {
            SetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_KEYNB, value);
        }

        public void SetFileNumber(byte value)
        {
            SetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_FILENB, value);
        }

        public void SetOffset(byte value)
        {
            SetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_OFFSET, value);
        }

        public void SetSize(byte value)
        {
            SetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_IDSIZE, value);
        }

        public void SetEncryption(SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireEncryptMode value)
        {
            SetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_ENC, (byte)value);
        }

        public void SetCommunication(SAM_SESymmetricKeyEntryDESFireProperties.SAM_SEDESFireCommunicationMode value)
        {
            SetMetadataByte(SAM_SEMetadataIndex.META_BYTE_DES_COMM, (byte)value);
        }

        public void SetEv0(bool value)
        {
            SetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_EV0, value);
        }

        public void SetEv1(bool value)
        {
            SetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_EV1, value);
        }

        public void SetEv2(bool value)
        {
            SetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_EV2, value);
        }

        public void SetAuthEv2(bool value)
        {
            SetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_AUTHEV2, value);
        }

        public void SetEv3(bool value)
        {
            SetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_EV3, value);
        }

        public void SetProximityCheck(bool value)
        {
            SetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_PROXCHECK, value);
        }

        public void SetJcop(bool value)
        {
            SetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_JCOP, value);
        }

        public void SetJcopEv3(bool value)
        {
            SetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DES_JCOP_EV3, value);
        }

        public void SetDivEnable(bool value)
        {
            SetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DIV_ACTIVE, value);
        }

        public void SetDivAidInv(bool value)
        {
            SetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DIV_INVAID, value);
        }

        [DllImport(SAM_SEDllConstants.SPSEDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int spse_setMetaDivSysId(UIntPtr context, uint keyId, byte[] Si, int size, bool incKeySi, byte keySi);
        private void SetMetaDivSysId(byte[] value, bool incKeySi, byte keySi)
        {
            SAM_SEDllConstants.ErrorHandler.HandlingError(spse_setMetaDivSysId(Context, Id, value, value.Length, incKeySi, keySi));
        }

        public void SetDivSi(byte[] Si, bool keyInc, byte keyNum)
        {
            SetMetaDivSysId(Si, keyInc, keyNum);
        }
    }
}
