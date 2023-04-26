namespace Leosac.KeyManager.Library.Policy
{
    public interface IKeyPolicy
    {
        void Validate(Key key);

        void Validate(string? value);
    }
}
