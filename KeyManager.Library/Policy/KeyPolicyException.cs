namespace Leosac.KeyManager.Library.Policy
{
    public class KeyPolicyException : Exception
    {
        public KeyPolicyException(string? message) : base(message) { }

        public KeyPolicyException(string? message, Exception? exception) : base(message, exception) { }
    }
}
