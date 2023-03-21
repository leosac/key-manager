using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.Plugin
{
    public abstract class KeyEntryUIFactory : KMFactory<KeyEntryUIFactory>
    {
        protected KeyEntryFactory? targetFactory;

        public abstract UserControl CreateKeyEntryPropertiesControl();

        public abstract KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel();

        public KeyEntryFactory? TargetFactory => targetFactory;
    }
}
