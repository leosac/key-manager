using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.Plugin
{
    /// <summary>
    /// The base class for Key Store UI Factory implementation.
    /// </summary>
    public abstract class KeyStoreUIFactory : KMFactory<KeyStoreUIFactory>
    {
        protected KeyStoreFactory? targetFactory;

        /// <summary>
        /// Create a new UI control instance for the Key Store Properties.
        /// </summary>
        /// <returns>The control instance.</returns>
        public abstract UserControl CreateKeyStorePropertiesControl();

        /// <summary>
        /// Create a new Control View Model instance for the Key Store Properties UI control.
        /// </summary>
        /// <returns>The Control View Model instance/</returns>
        public abstract KeyStorePropertiesControlViewModel? CreateKeyStorePropertiesControlViewModel();

        /// <summary>
        /// Get list of new UI controls instances of additionals key store UI tabs, if any.
        /// </summary>
        /// <returns>The list of controls instances.</returns>
        public abstract IDictionary<string, UserControl> CreateKeyStoreAdditionalControls();

        public KeyStoreFactory? TargetFactory => targetFactory;
    }
}
