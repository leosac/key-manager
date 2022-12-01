using Leosac.KeyManager;
using log4net;
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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public App()
        {
            Leosac.KeyManager.Library.UI.KeyStoreFactory.Register(new Leosac.KeyManager.Library.KeyStore.Memory.UI.MemoryKeyStoreFactory());
            Leosac.KeyManager.Library.UI.KeyStoreFactory.Register(new Leosac.KeyManager.Library.KeyStore.File.UI.FileKeyStoreFactory());
            Leosac.KeyManager.Library.UI.KeyStoreFactory.Register(new Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.SAMKeyStoreFactory());
            Leosac.KeyManager.Library.UI.KeyStoreFactory.Register(new Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.PKCSC11KeyStoreFactory());

            Leosac.KeyManager.Library.UI.KeyEntryFactory.Register(new Leosac.KeyManager.Library.KeyStore.Memory.UI.MemoryKeyEntryFactory());
            Leosac.KeyManager.Library.UI.KeyEntryFactory.Register(new Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.SAMSymmetricKeyEntryFactory());
            Leosac.KeyManager.Library.UI.KeyEntryFactory.Register(new Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.PKCS11KeyEntryFactory());
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            log.Debug("Starting up the application...");

            var settings = KMSettings.GetSingletonInstance();
            if (!string.IsNullOrEmpty(settings.Language))
            {
                LangHelper.ChangeLanguage(settings.Language);
            }
        }
    }
}
