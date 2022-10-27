using Leosac.KeyManager.Library.Policy;

namespace Leosac.KeyManager.Library
{
    public class Key
    {
        public Key()
        {
            Policies = new List<IKeyPolicy>();
        }

        public int Version { get; set; } = 0;

        public string Value { get; private set; } = string.Empty;

        public IList<IKeyPolicy> Policies { get; set; }
    }
}