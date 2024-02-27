namespace Leosac.KeyManager.Library.KeyGen.SecretSharing
{
    public class XorSecretSharing : SecretSharingBase
    {
        public override string Name => "Xor";

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

            var chunks = new byte[nbFragments][];
            for (int c = 0; c < nbFragments; ++c)
            {
                chunks[c] = KeyGeneration.Random((uint)secret.Length);
            }
            for (int i = 0; i < secret.Length; ++i)
            {
                var b = secret[i];
                for (int c = 1; c < nbFragments; ++c)
                {
                    b ^= chunks[c][i];
                }
                chunks[0][i] = b;
            }

            var fragments = new string[nbFragments];
            for (int c = 0; c < nbFragments; ++c)
            {
                fragments[c] = Convert.ToHexString(chunks[c]);
            }
            return fragments;
        }

        public override byte[]? ComputeFragments(string[] fragments)
        {
            if (fragments.Length == 0)
            {
                return null;
            }

            var key = new byte[fragments[0].Length / 2];
            foreach (string fragment in fragments)
            {
                var keyb = Convert.FromHexString(fragment);
                for (int i = 0; i < key.Length && i < keyb.Length; ++i)
                {
                    key[i] ^= keyb[i];
                }
            }
            return key;
        }
    }
}
