using System.Text;

namespace Leosac.KeyManager.Library.DivInput
{
    public class KeyEntryIdDivInputFragment : DivInputFragment
    {
        public override string GetFragment(DivInputContext context)
        {
            // TODO: we may want to have a property to define the input conversion (eg. to use decimal string as byte value, ...)
            return Convert.ToHexString(Encoding.UTF8.GetBytes(context.KeyEntry!.Identifier.Id!));
        }
    }
}
