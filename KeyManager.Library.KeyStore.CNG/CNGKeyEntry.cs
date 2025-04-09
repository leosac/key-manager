using Newtonsoft.Json;
using Vanara.PInvoke;

namespace Leosac.KeyManager.Library.KeyStore.CNG
{
    public class CNGKeyEntry : KeyEntry
    {
        public CNGKeyEntry() : this(KeyEntryClass.Symmetric)
        {

        }

        public CNGKeyEntry(KeyEntryClass kclass)
        {
            Properties = new CNGKeyEntryProperties();
            _kclass = kclass;
            Identifier.Id = Guid.NewGuid().ToString("N");
        }

        private readonly KeyEntryClass _kclass;

        [JsonIgnore]
        public CNGKeyEntryProperties? CNGProperties
        {
            get { return Properties as CNGKeyEntryProperties; }
        }

        public override KeyEntryClass KClass => _kclass;

        public static IEnumerable<string> GetSymmetricAlgIds()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            return
            [
                BCrypt.StandardAlgorithmId.BCRYPT_3DES_112_ALGORITHM,
                BCrypt.StandardAlgorithmId.BCRYPT_3DES_ALGORITHM,
                BCrypt.StandardAlgorithmId.BCRYPT_AES_ALGORITHM,
                BCrypt.StandardAlgorithmId.BCRYPT_DES_ALGORITHM,
                BCrypt.StandardAlgorithmId.BCRYPT_DESX_ALGORITHM,
                BCrypt.StandardAlgorithmId.BCRYPT_RC2_ALGORITHM,
                "HMAC-SHA256"
            ];
#pragma warning restore CA1416 // Validate platform compatibility
        }

        public static IEnumerable<string> GetAsymmetricAlgIds()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            return
            [
                BCrypt.StandardAlgorithmId.BCRYPT_DSA_ALGORITHM,
                BCrypt.StandardAlgorithmId.BCRYPT_ECDSA_ALGORITHM,
                BCrypt.StandardAlgorithmId.BCRYPT_ECDSA_P256_ALGORITHM,
                BCrypt.StandardAlgorithmId.BCRYPT_ECDSA_P384_ALGORITHM,
                BCrypt.StandardAlgorithmId.BCRYPT_ECDSA_P521_ALGORITHM,
                BCrypt.StandardAlgorithmId.BCRYPT_RSA_ALGORITHM
            ];
#pragma warning restore CA1416 // Validate platform compatibility
        }

        public static KeyEntryClass GetKeyEntryClassFromAlgId(string algId)
        {
            algId = algId.ToUpper();
            if (GetSymmetricAlgIds().Contains(algId))
            {
                return KeyEntryClass.Symmetric;
            }

            return KeyEntryClass.Asymmetric;
        }

        public override IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter)
        {
            var variants = new List<KeyEntryVariant>();

            if (classFilter == null || classFilter == KeyEntryClass.Symmetric)
            {
                var algIds = GetSymmetricAlgIds();
                foreach (var algId in algIds)
                {
                    var algvar = new KeyEntryVariant { Name = algId };
                    algvar.KeyContainers.Add(new KeyVersion("Key", 0, new Key(new[] { algId, KeyEntryClass.Symmetric.ToString() })));
                    variants.Add(algvar);
                }
            }

            if (classFilter == null || classFilter == KeyEntryClass.Asymmetric)
            {
                var algIds = GetAsymmetricAlgIds();
                foreach (var algId in algIds)
                {
                    var algvar = new KeyEntryVariant { Name = algId };
                    algvar.KeyContainers.Add(new KeyVersion("Key", 0, new Key(new[] { algId, KeyEntryClass.Asymmetric.ToString() })));
                    variants.Add(algvar);
                }
            }

            return variants;
        }
    }
}
