namespace Leosac.KeyManager.Library
{
    public class Key
    {
        public string Identifier { get; set; } = new Guid().ToString();

        public int Version { get; set; } = 0;

        public string Value { get; private set; } = string.Empty;
    }
}