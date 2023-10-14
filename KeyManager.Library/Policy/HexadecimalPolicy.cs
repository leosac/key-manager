namespace Leosac.KeyManager.Library.Policy
{
    public class HexadecimalPolicy : IKeyPolicy
    {
        public void Validate(Key key)
        {
            foreach(var k in key.Materials)
            {
                Validate(k.Value);
            }
        }

        public void Validate(string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Convert.FromHexString(value);
            }
        }
    }
}
