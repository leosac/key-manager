using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.DivInput
{
    public abstract class DivInputFragment : ObservableValidator
    {
        public abstract string GetFragment(DivInputContext context);
    }
}
