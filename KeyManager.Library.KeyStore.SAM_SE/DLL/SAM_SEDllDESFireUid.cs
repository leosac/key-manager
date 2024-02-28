namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL
{
    public class SAM_SEDllDESFireUid(UIntPtr ctx, string stringId, uint id, SAM_SESymmetricKeyEntryProperties.SAM_SEKeyEntryType type, SAM_SEDllErrorHandler errorHandler) : SAM_SEDllObject(ctx, stringId, id, type, errorHandler)
    {
        public bool GetUidActive()
        {
            return GetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DIV_ACTIVE);
        }

        public byte GetUidKeyNumber()
        {
            return GetMetadataByte(SAM_SEMetadataIndex.META_BYTE_UID_KEYNB);
        }

        public void SetUidEnable(bool value)
        {
            SetMetadataBool(SAM_SEMetadataIndex.META_BOOL_DIV_ACTIVE, value);
        }

        public void SetUidKeyNumber(byte value)
        {
            SetMetadataByte(SAM_SEMetadataIndex.META_BYTE_UID_KEYNB, value);
        }
    }
}
