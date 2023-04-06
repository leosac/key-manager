using Leosac.KeyManager;
using Leosac.KeyManager.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.UI;
using Leosac.WpfApp;
using Leosac.WpfApp.Domain;
using log4net;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
            KMPlugins.Load();
            Favorites.SingletonCreated += (sender, e) => {
                // We recreate the Key Store Properties instances on the appropriate AssemblyLoadContext.
                // This is kind of a hack, it would be cleaner to handle proper context instanciation on Newtonsoft.Json deserialization directly but it's not really made for it.
                // Another way would be to separate Favorites config into individual config files loaded by the key store factories running already on the correct context.
                (sender as Favorites)?.KeyStores.ToList().ForEach(f =>
                {
                    var factory = KeyStoreFactory.GetFactoryFromPropertyType(f.Properties?.GetType());
                    if (factory != null)
                    {
                        f.Properties = factory.CreateKeyStoreProperties(JsonConvert.SerializeObject(f.Properties));
                    }
                });
            };
            LeosacAppInfo.Instance = new KMLeosacAppInfo();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            log.Debug("Starting up the application...");

            var settings = AppSettings.GetSingletonInstance();
            if (!string.IsNullOrEmpty(settings.Language))
            {
                LangHelper.ChangeLanguage(settings.Language);
            }
        }
    }
}
