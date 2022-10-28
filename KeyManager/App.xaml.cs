using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KeyManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Leosac.KeyManager.Library.UI.KeyStoreFactory.Register(new Leosac.KeyManager.Library.KeyStore.Memory.UI.MemoryKeyStoreFactory());
            Leosac.KeyManager.Library.UI.KeyStoreFactory.Register(new Leosac.KeyManager.Library.KeyStore.File.UI.FileKeyStoreFactory());
            Leosac.KeyManager.Library.UI.KeyStoreFactory.Register(new Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.SAMKeyStoreFactory());
            Leosac.KeyManager.Library.UI.KeyStoreFactory.Register(new Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.PKCSC11KeyStoreFactory());

            Leosac.KeyManager.Library.UI.KeyEntryFactory.Register(new Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.DESSAMKeyEntryFactory());
            Leosac.KeyManager.Library.UI.KeyEntryFactory.Register(new Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.AES128SAMKeyEntryFactory());
            Leosac.KeyManager.Library.UI.KeyEntryFactory.Register(new Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.T3KDESSAMKeyEntryFactory());
        }
    }
}
