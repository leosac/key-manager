namespace Leosac.KeyManager.Library.SecretSharing
{
    public abstract class SecretSharingBase
    {
        public abstract string Name { get; }

        public abstract string[]? CreateFragments(byte[]? secret, int nbFragments);

        public abstract byte[]? ComputeFragments(string[] fragments);

        public static IList<SecretSharingBase> GetAll()
        {
            return new List<SecretSharingBase>
            {
                new ShamirsSecretSharing(),
                new ConcatSecretSharing(),
                new XorSecretSharing()
            };
        }
    }
}
