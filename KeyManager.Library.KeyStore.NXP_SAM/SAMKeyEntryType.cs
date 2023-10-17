namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public enum SAMKeyEntryType : byte
    {
        Host = 0x00,
        PICC = 0x01,
        OfflineChange = 0x02,
        OfflineCrypto = 0x04
    }
}
