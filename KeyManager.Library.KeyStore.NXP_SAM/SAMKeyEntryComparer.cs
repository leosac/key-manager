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
            if (x == null && y == null)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            if (x.Identifier.Id == _properties.AuthenticateKeyEntryIdentifier.ToString())
                return 1;

            if (y.Identifier.Id == _properties.AuthenticateKeyEntryIdentifier.ToString())
                return -1;

            try
            {
                return int.Parse(x.Identifier.Id ?? "0").CompareTo(int.Parse(y.Identifier.Id ?? "0"));
            }
            catch
            {
                return 0;
            }
        }

    }
}
