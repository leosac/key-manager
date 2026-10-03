namespace Leosac.KeyManager.Library.Policy
{
    public class KeyLengthPolicy : IKeyPolicy
    {
        public KeyLengthPolicy(uint bitLength)
        {
            BitLength = bitLength;
        }

        public void Validate(Key key)
        {
            foreach (var material in key.Materials)
            {
                Validate(material.Value);
            }
        }

        public void Validate(string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Length % 2 != 0)
                {
                    throw new KeyPolicyException("Key is not correctly formated to be parsed to a byte array.");
                }

                if (value.Length * 4 != BitLength)
                {
                    throw new KeyPolicyException("Wrong key length.");
                }
            }
        }

        public uint BitLength { get; set; }

        public uint ByteLength
        {
            get => checked((BitLength + 7) / 8);
            set => BitLength = checked(value * 8);
        }
    }
}
