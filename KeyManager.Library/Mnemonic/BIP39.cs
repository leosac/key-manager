using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Leosac.KeyManager.Library.Mnemonic
{
    /// <summary>
    /// Mnemonic calculation.
    /// Based on https://github.com/elucidsoft/dotnetstandard-bip39 (Apache License 2.0)
    /// </summary>
    public partial class BIP39
    {
        public string MnemonicToEntropy(string mnemonic, WordlistLang lang)
        {
            var wordlist = GetWordList(lang);
            var words = mnemonic.Normalize(NormalizationForm.FormKD).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length % 3 != 0)
            {
                throw new MnemonicException("Invalid mnemonic");
            }

            var bits = string.Join("", words.Select(word =>
            {
                var index = Array.IndexOf(wordlist, word);
                if (index == -1)
                {
                    throw new MnemonicException("Invalid mnemonic");
                }

                return Convert.ToString(index, 2).PadLeft(11, '0');
            }));

            // split the binary string into ENT/CS
            var dividerIndex = (int)Math.Floor((double)bits.Length / 33) * 32;
            var entropyBits = bits[..dividerIndex];
            var checksumBits = bits[dividerIndex..];

            // calculate the checksum and compare
            var entropyBytesMatch = BitsRegex().Matches(entropyBits)
                .OfType<Match>()
                .Select(m => m.Groups[0].Value)
                .ToArray();

            var entropyBytes = entropyBytesMatch
                .Select(bytes => Convert.ToByte(bytes, 2)).ToArray();

            CheckValidEntropy(entropyBytes);

            var newChecksum = DeriveChecksumBits(entropyBytes);
            if (newChecksum != checksumBits)
            {
                throw new MnemonicException("Invalid mnemonic checksum");
            }

            return Convert.ToHexString(entropyBytes);
        }

        public string EntropyToMnemonic(string entropy, WordlistLang lang)
        {
            var wordList = GetWordList(lang);

            //How can I do this more efficiently, the multiple substrings I don't like...
            var entropyBytes = Enumerable.Range(0, entropy.Length / 2)
                .Select(x => Convert.ToByte(entropy.Substring(x * 2, 2), 16))
                .ToArray();

            CheckValidEntropy(entropyBytes);

            var entropyBits = BytesToBinary(entropyBytes);
            var checksumBits = DeriveChecksumBits(entropyBytes);

            var bits = entropyBits + checksumBits;

            var chunks = ChunksRegex().Matches(bits)
                .OfType<Match>()
                .Select(m => m.Groups[0].Value)
                .ToArray();

            var words = chunks.Select(binary =>
            {
                var index = Convert.ToInt32(binary, 2);
                return wordList[index];
            });

            return string.Join(" ", words);
        }

        public string GenerateMnemonic(int strength, WordlistLang wordListType)
        {
            if (strength % 32 != 0)
            {
                throw new MnemonicException("Invalid Entropy");
            }

            using var random = RandomNumberGenerator.Create();
            byte[] buffer = new byte[strength / 8];
            random.GetBytes(buffer);
            var entropyHex = Convert.ToHexString(buffer);
            return EntropyToMnemonic(entropyHex, wordListType);
        }

        private void CheckValidEntropy(byte[] entropyBytes)
        {
            if (entropyBytes == null)
            {
                throw new ArgumentNullException(nameof(entropyBytes));
            }

            if (entropyBytes.Length < 16)
            {
                throw new MnemonicException("Invalid Entropy (Length < 16)");
            }

            if (entropyBytes.Length > 32)
            {
                throw new MnemonicException("Invalid Entropy (Length > 32)");
            }

            if (entropyBytes.Length % 4 != 0)
            {
                throw new MnemonicException("Invalid Entropy(Length % 4 != 0");
            }
        }

        private string Salt(string password)
        {
            return "mnemonic" + (!string.IsNullOrEmpty(password) ? password : "");
        }

        private byte[] MnemonicToSeed(string mnemonic, string password)
        {
            return MnemonicToSeed(mnemonic, password, 64);
        }

        private byte[] MnemonicToSeed(string mnemonic, string password, int keySize)
        {
            if (mnemonic == null)
            {
                throw new ArgumentNullException(nameof(mnemonic));
            }

            var mnemonicBytes = Encoding.UTF8.GetBytes(mnemonic.Normalize(NormalizationForm.FormKD));
            var saltBytes = Encoding.UTF8.GetBytes(Salt(password.Normalize(NormalizationForm.FormKD)));

            var deriv = new Rfc2898DeriveBytes(mnemonicBytes, saltBytes, 2048, HashAlgorithmName.SHA512);
            return deriv.GetBytes(keySize);
        }

        public string MnemonicToSeedHex(string mnemonic, string password)
        {
            return MnemonicToSeedHex(mnemonic, password, 64);
        }

        public string MnemonicToSeedHex(string mnemonic, string password, int keySize)
        {
            var key = MnemonicToSeed(mnemonic, password, keySize);
            return Convert.ToHexString(key);
        }

        private string DeriveChecksumBits(byte[] checksum)
        {
            var ent = checksum.Length * 8;
            var cs = ent / 32;
            var hash = SHA256.HashData(checksum);
            string result = BytesToBinary(hash);
            return result[..cs];
        }

        private string BytesToBinary(byte[] hash)
        {
            return string.Join("", hash.Select(h => Convert.ToString(h, 2).PadLeft(8, '0')));
        }

        private string[] GetWordList(WordlistLang lang)
        {
            var wordlistFs = Assembly.GetAssembly(typeof(WordlistLang))?
                .GetManifestResourceStream($"{typeof(WordlistLang).Namespace}.Words.{lang}.txt");

            var words = new List<string>();
            using (var reader = new StreamReader(wordlistFs ?? throw new MnemonicException($"could not load word list {lang}")))
            {
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        words.Add(line);
                    }
                }
            }

            return words.ToArray();
        }

        [GeneratedRegex("(.{1,8})")]
        private static partial Regex BitsRegex();
        [GeneratedRegex("(.{1,11})")]
        private static partial Regex ChunksRegex();
    }
}
