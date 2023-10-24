namespace Leosac.KeyManager.Library.SecretSharing
{
    public class ConcatSecretSharing : SecretSharingBase
    {
        public override string Name => "Concatenation";

        public override string[]? CreateFragments(byte[]? secret, int nbFragments)
        {
            if (secret == null || secret.Length == 0)
            {
                return null;
            }
            if (secret.Length < nbFragments)
            {
                throw new ArgumentException("The number of fragments exceed the secret length.", nameof(nbFragments));
            }
            if (nbFragments < 2)
            {
                throw new ArgumentException("The number of fragments should at least be 2.", nameof(nbFragments));
            }

            var str = Convert.ToHexString(secret);
            var fragments = new string[nbFragments];
            var length = (int)Math.Ceiling((double)str.Length / nbFragments);
            for (int i = 0; i < nbFragments; i++)
            {
                var clength = (i + 1) < nbFragments ? length : (str.Length - i * length);
                fragments[i] = str.Substring(i * length, clength);
            }
            return fragments;
        }

        public override byte[]? ComputeFragments(string[] fragments)
        {
            return Convert.FromHexString(string.Join("", fragments));
        }
    }
}
