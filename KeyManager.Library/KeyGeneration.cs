using System.Security.Cryptography;
using System.Text;
using System.Collections.Concurrent;

namespace Leosac.KeyManager.Library
{
    public interface IKeyGenerator
    {
        IEnumerable<KeyMaterial> Generate(Key key, uint explicitKeySize = 0);
    }

    public static class KeyGeneration
    {
        private static readonly ConcurrentDictionary<string, IKeyGenerator> Generators = new(StringComparer.OrdinalIgnoreCase);

        static KeyGeneration()
        {
            Register("AES", new SymmetricKeyGenerator(128, new HashSet<uint> { 128, 192, 256 }));
            Register("DES", new SymmetricKeyGenerator(64, new HashSet<uint> { 64, 128, 192 }));
            Register("RSA", new RsaKeyGenerator());
            Register("ECC", new EccKeyGenerator());
            Register("ECDSA", new EccKeyGenerator());
            Register("ECDH", new EccKeyGenerator());
        }

        public static byte[] Random(uint keySize)
        {
            using var rng = RandomNumberGenerator.Create();
            var key = new byte[keySize];
            rng.GetBytes(key);
            return key;
        }

        public static byte[] RandomBits(uint keySize)
        {
            return Random(BitsToBytes(keySize));
        }

        public static uint BitsToBytes(uint keySize)
        {
            return checked((keySize + 7) / 8);
        }
        public static uint BytesToBits(uint keySize)
        {
            return checked(keySize * 8);
        }

        public static byte[] FromPassword(string password, string salt, uint keySize)
        {
            return Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt), 10000, HashAlgorithmName.SHA256, (int)keySize);
        }

        public static byte[] CreateRandomSalt(uint length)
        {
            return Random(length >= 1 ? length : 1);
        }

        public static void Register(string algorithm, IKeyGenerator generator)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(algorithm);
            ArgumentNullException.ThrowIfNull(generator);
            Generators[algorithm] = generator;
        }

        public static bool Unregister(string algorithm)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(algorithm);
            return Generators.TryRemove(algorithm, out _);
        }

        public static IEnumerable<KeyMaterial> Generate(Key key, uint keySize = 0)
        {
            ArgumentNullException.ThrowIfNull(key);
            foreach (var tag in key.Tags)
            {
                if (Generators.TryGetValue(tag, out var generator))
                {
                    return generator.Generate(key, keySize) ?? throw new InvalidOperationException($"The key generator registered for '{tag}' returned no materials.");
                }
            }

            throw new NotSupportedException("The key does not contain a registered key-generation tag.");
        }

        public static IEnumerable<KeyMaterial> Generate(Key key, string algorithm, uint keySize = 0)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentException.ThrowIfNullOrWhiteSpace(algorithm);
            if (!Generators.TryGetValue(algorithm, out var generator))
            {
                throw new NotSupportedException($"No key generator is registered for '{algorithm}'.");
            }

            return generator.Generate(key, keySize) ?? throw new InvalidOperationException($"The key generator registered for '{algorithm}' returned no materials.");
        }

        private sealed class SymmetricKeyGenerator(uint defaultSize, IReadOnlySet<uint> supportedSizes) : IKeyGenerator
        {
            public IEnumerable<KeyMaterial> Generate(Key key, uint explicitKeySize = 0)
            {
                var size = explicitKeySize == 0 ? (key.KeySize == 0 ? defaultSize : key.KeySize) : explicitKeySize;
                if (!supportedSizes.Contains(size))
                {
                    throw new ArgumentOutOfRangeException(nameof(key.KeySize), size, "The key size is not supported by this algorithm.");
                }

                yield return new KeyMaterial(Convert.ToHexString(RandomBits(size)));
            }
        }

        private sealed class RsaKeyGenerator : IKeyGenerator
        {
            public IEnumerable<KeyMaterial> Generate(Key key, uint explicitKeySize = 0)
            {
                var size = explicitKeySize == 0 ? (key.KeySize == 0 ? 2048 : key.KeySize) : explicitKeySize;
                using var rsa = RSA.Create(checked((int)size));
                yield return CreateDerMaterial(KeyMaterial.PRIVATE_KEY, rsa.ExportPkcs8PrivateKey());
                yield return CreateDerMaterial(KeyMaterial.PUBLIC_KEY, rsa.ExportSubjectPublicKeyInfo());
            }
        }

        private sealed class EccKeyGenerator : IKeyGenerator
        {
            public IEnumerable<KeyMaterial> Generate(Key key, uint explicitKeySize = 0)
            {
                var size = explicitKeySize == 0 ? key.KeySize : explicitKeySize;
                var curve = size switch
                {
                    0 or 256 => ECCurve.NamedCurves.nistP256,
                    384 => ECCurve.NamedCurves.nistP384,
                    521 => ECCurve.NamedCurves.nistP521,
                    _ => throw new ArgumentOutOfRangeException(nameof(key.KeySize), key.KeySize, "ECC key size must be 256, 384, or 521 bits.")
                };

                using var ecc = ECDsa.Create(curve);
                yield return CreateDerMaterial(KeyMaterial.PRIVATE_KEY, ecc.ExportPkcs8PrivateKey());
                yield return CreateDerMaterial(KeyMaterial.PUBLIC_KEY, ecc.ExportSubjectPublicKeyInfo());
            }
        }

        private static KeyMaterial CreateDerMaterial(string name, byte[] value)
        {
            return new KeyMaterial(Convert.ToBase64String(value), name)
            {
                ValueFormat = KeyValueStringFormat.Der
            };
        }
    }
}
