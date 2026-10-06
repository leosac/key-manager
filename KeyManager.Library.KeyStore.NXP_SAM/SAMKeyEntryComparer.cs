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

            var classComparison = GetClassOrder(x.KClass).CompareTo(GetClassOrder(y.KClass));
            if (classComparison != 0)
                return classComparison;

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

        private static int GetClassOrder(KeyEntryClass keyEntryClass)
        {
            return keyEntryClass switch
            {
                KeyEntryClass.Asymmetric => 0,
                KeyEntryClass.Symmetric => 1,
                _ => 2 + (int)keyEntryClass
            };
        }

    }
}
