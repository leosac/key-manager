using System.Numerics;

namespace Leosac.KeyManager.Library.SecretSharing
{
    public class ShamirsSecretSharing : SecretSharingBase
    {
        public override string Name => "Shamir's Secret Sharing";

        public override string[]? CreateFragments(byte[]? secret, int nbFragments)
        {
            if (secret == null || secret.Length == 0)
            {
                return null;
            }
            if (nbFragments < 3)
            {
                throw new ArgumentException("The number of fragments should at least be 3.", nameof(nbFragments));
            }
            var min = (int)Math.Ceiling((double)nbFragments * 2 / 3);
            var gcd = new SecretSharingDotNet.Math.ExtendedEuclideanAlgorithm<BigInteger>();
            var sss = new SecretSharingDotNet.Cryptography.ShamirsSecretSharing<BigInteger>(gcd);
            var shares = sss.MakeShares(min, nbFragments, secret);
            var fragments = new string[shares.Count];
            for (int i = 0; i < fragments.Length; ++i)
            {
                fragments[i] = shares[i].ToString();
            }
            return fragments;
        }

        public override byte[]? ComputeFragments(string[] fragments)
        {
            var gcd = new SecretSharingDotNet.Math.ExtendedEuclideanAlgorithm<BigInteger>();
            var combine = new SecretSharingDotNet.Cryptography.ShamirsSecretSharing<BigInteger>(gcd);
            var shares = string.Join(Environment.NewLine, fragments);
            var secret = combine.Reconstruction(shares);
            return secret.ToByteArray();
        }
    }
}
