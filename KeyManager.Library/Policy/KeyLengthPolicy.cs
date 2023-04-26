namespace Leosac.KeyManager.Library.Policy
{
    public class KeyLengthPolicy : IKeyPolicy
    {
        public KeyLengthPolicy(uint byteLength)
        {
            ByteLength = byteLength;
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
                    throw new KeyPolicyException("Key is not correctly formated to be parsed to a byte array.");

                if (value.Length / 2 != ByteLength)
                    throw new KeyPolicyException("Wrong key length.");
            }
        }

        public uint ByteLength { get; set; }
    }
}
