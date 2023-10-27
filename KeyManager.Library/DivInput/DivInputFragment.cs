using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.DivInput
{
    public abstract class DivInputFragment : ObservableObject, ICloneable
    {
        public abstract string Name { get; }

        public abstract object Clone();

        public abstract string GetFragment(DivInputContext context);

        public override string ToString()
        {
            return Name;
        }

        public static IList<DivInputFragment> GetAll()
        {
            // Use reflexion / factories here instead?
            return new List<DivInputFragment>
            {
                new KeyEntryIdDivInputFragment(),
                new KeyVersionDivInputFragment(),
                new PaddingDivInputFragment(),
                new RandomDivInputFragment(),
                new StaticDivInputFragment(),
                new KeyStoreAttributeDivInputFragment()
            };
        }
    }
}
