namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMKeyEntryComparer : IComparer<IChangeKeyEntry>
    {
        private readonly SAMKeyStoreProperties _properties;
        public SAMKeyEntryComparer(SAMKeyStoreProperties properties)
        {
            _properties = properties;
        }

        public int Compare(IChangeKeyEntry? x, IChangeKeyEntry? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            if (x is SAMSymmetricKeyEntry kx)
            {
                // Always push to end the key used for key store authentication
                if (kx.Identifier.Id == _properties.AuthenticateKeyEntryIdentifier.ToString())
                {
                    return 1;
                }

                if (y is SAMSymmetricKeyEntry ky)
                {
                    try
                    {
                        return int.Parse(kx.Identifier.Id ?? "0") - int.Parse(ky.Identifier.Id ?? "0");
                    }
                    catch
                    {
                        return 0;
                    }
                }

                return 1;
            }

            return 0;
        }
    }
}
