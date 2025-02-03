/*
** File Name: SAM_SEDllDESFireUid.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file exposes methods from DLL which works with DESFire UID object.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL
{
    public class SAM_SEDllDESFireUid(UIntPtr ctx, string stringId, uint id, SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType type) : SAM_SEDllObject(ctx, stringId, id, type)
    {
        public bool GetUidEnable()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_UID_ACTIVE);
        }

        public byte GetUidKeyNumber()
        {
            return GetMetadataByte(SAM_SEMetadataIndex.META_BYTE_UID_KEYNB);
        }

        public void SetUidEnable(bool value)
        {
            SetMetadataBool(SAM_SEMetadataIndex.META_BOOL_UID_ACTIVE, value);
        }

        public void SetUidKeyNumber(byte value)
        {
            SetMetadataByte(SAM_SEMetadataIndex.META_BYTE_UID_KEYNB, value);
        }
    }
}
