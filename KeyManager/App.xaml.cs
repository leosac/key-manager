using Leosac.KeyManager.Library.UI;
using Leosac.SharedServices;
using Leosac.WpfApp;
using System;
using System.Windows;

namespace Leosac.KeyManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public App()
        {
            InitializeApplication();
        }

        private static void InitializeApplication()
        {
            InitializeApplicationInfo();
            InitializeUserRole();
            InitializePlugins();
            InitializeFavorites();
        }

        private static void InitializeUserRole()
        {
            try
            {
                UserRoleContext.Initialize(KMSettings.LoadFromFile(false));
            }
            catch (Exception ex)
            {
                log.Error("Failed to initialize user role.", ex);
                UserRoleContext.Initialize(null);
            }

            if (UserRoleContext.IsEnabled)
            {
                log.Debug($"User role context initialized. User: {UserRoleContext.UserName}, Role: {UserRoleContext.Role}");
                if (UserRoleContext.Role == UserRole.None)
                {
                    log.Debug("The user doesn't have a role, leaving the application.");
                    Application.Current.Shutdown();
                }
            }
            else
            {
                log.Debug("User role context is disabled.");
            }
        }

        private static void InitializeApplicationInfo()
        {
            LeosacAppInfo.Instance = new KMLeosacAppInfo();
        }

        private static void InitializePlugins()
        {
            try
            {
                KMPlugins.Load();
            }
            catch (Exception ex)
            {
                log.Error("Error while loading plugins.", ex);
                throw;
            }
        }

        private static void InitializeFavorites()
        {
            try
            {
                _ = FavoritesManager.User;
                _ = FavoritesManager.Shared;
            }
            catch (Exception ex)
            {
                log.Error("Failed to initialize favorites.", ex);
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            log.Debug("Starting up the application...");

            var settings = UserAppSettings.GetSingletonInstance();
            if (!string.IsNullOrEmpty(settings.Language))
                LangHelper.ChangeLanguage(settings.Language);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Clipboard.Clear();
            }
            catch (Exception ex)
            {
                log.Debug("Failed to clear clipboard.", ex);
            }
        }
    }
}
