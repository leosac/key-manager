using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Domain;
using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
using Leosac.WpfApp.Domain;
using System;
using System.Windows.Controls;

namespace Leosac.KeyManager
{
    public class KMLeosacAppInfo : LeosacWinAppInfo
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public KMLeosacAppInfo()
        {
            ApplicationName = "Key Manager";
            ApplicationTitle = "Leosac Key Manager";
            ApplicationCode = "lkm";
            ApplicationUrl = "https://leosac.com/key-manager/";
            ApplicationLogo = "/images/leosac_key.png";
            SettingsCommand = new RelayCommand(() =>
            {
                var settings = KMSettings.LoadFromFile(false);
                if (settings != null && settings.EnsureElevation())
                {
                    var settingsWindow = new SettingsWindow();
                    settingsWindow.DataContext = settings;
                    settingsWindow.ShowDialog();
                }
            });
            PerUserInstallation = null; // Automatic decision based on program installation
        }

        public override void InitializeMainWindow(MainWindowViewModel model)
        {
            var HomeCommand = new RelayCommand(
                () =>
                {
                    model.SelectedIndex = 0;
                });
            var FavoritesCommand = new RelayCommand(
                () =>
                {
                    model.SelectedIndex = 1;
                });
            var KeyStoreCommand = new AsyncRelayCommand<object>(
                async parameter =>
                {
                    if (parameter != null)
                    {
                        KeyStore? ks = null;
                        Favorite? fav = null;
                        KeyStoreUIFactory? factory = null;
                        if (parameter is KeyStore)
                        {
                            ks = parameter as KeyStore;
                        }
                        else if (parameter is Favorite)
                        {
                            fav = parameter as Favorite;
                            ks = fav?.CreateKeyStore();
                        }

                        if (ks != null)
                        {
                            factory = KeyStoreUIFactory.GetFactoryFromPropertyType(ks.Properties!.GetType());
                            if (factory != null)
                            {
                                try
                                {
                                    model.SelectedIndex = 2;
                                    if (model.SelectedItem?.DataContext is EditKeyStoreControlViewModel editModel)
                                    {
                                        // Ensure everything is back to original state
                                        editModel.CloseKeyStore(false);
                                        editModel.KeyStore = ks;
                                        editModel.Favorite = fav;
                                        await editModel.OpenKeyStore();

                                        var additionalControls = factory.CreateKeyStoreAdditionalControls();
                                        foreach (var addition in additionalControls)
                                        {
                                            if (addition.Value.DataContext is KeyStoreAdditionalControlViewModel additionalModel)
                                            {
                                                additionalModel.KeyStore = ks;
                                                additionalModel.SnackbarMessageQueue = model.SnackbarMessageQueue;
                                            }
                                            editModel.Tabs.Add(new TabItem { Header = addition.Key, Content = addition.Value });
                                        }
                                    }
                                }
                                catch (KeyStoreException ex)
                                {
                                    SnackbarHelper.EnqueueError(model.SnackbarMessageQueue, ex, "Key Store Error");
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Opening Key Store failed unexpected.", ex);
                                    SnackbarHelper.EnqueueError(model.SnackbarMessageQueue, ex);
                                }
                            }
                        }
                    }

                });

            model.MenuItems.Add(new NavItem(
                Properties.Resources.MenuHome,
                typeof(HomeControl),
                "House",
                new HomeControlViewModel(model.SnackbarMessageQueue)
                {
                    KeyStoreCommand = KeyStoreCommand,
                    FavoritesCommand = FavoritesCommand
                }
            ));
            model.MenuItems.Add(new NavItem(
                Properties.Resources.MenuFavorites,
                typeof(FavoritesControl),
                "Star",
                new FavoritesControlViewModel(model.SnackbarMessageQueue)
                {
                    KeyStoreCommand = KeyStoreCommand
                }
            ));
            model.MenuItems.Add(new NavItem(
                Properties.Resources.MenuKeyStore,
                typeof(EditKeyStoreControl),
                "ShieldKeyOutline",
                new EditKeyStoreControlViewModel(model.SnackbarMessageQueue)
                {
                    HomeCommand = HomeCommand
                }
            ));
        }

        public override void InitializeAboutWindow(AboutWindowViewModel model)
        {
            model.Libraries.Add(new AboutWindowViewModel.Library("SecretSharingDotNet", "MIT", "Shamir Secret Sharing library", "https://github.com/shinji-san/SecretSharingDotNet"));
            model.Libraries.Add(new AboutWindowViewModel.Library("Net.Codecrete.QrCodeGenerator", "MIT", "QR Code Generator library", "https://github.com/manuelbl/QrCodeGenerator"));
            model.Libraries.Add(new AboutWindowViewModel.Library("SkiaSharp", "MIT", "Graphic library", "https://github.com/mono/SkiaSharp"));
            model.Libraries.Add(new AboutWindowViewModel.Library("Pkcs11Interop", "Apache v2", "PKCS#11 libraries wrapper", "https://github.com/Pkcs11Interop/Pkcs11Interop"));
            model.Libraries.Add(new AboutWindowViewModel.Library("LibLogicalAccess", "LGPL", "RFID/NFC Library", "https://liblogicalaccess.com"));
            model.Libraries.Add(new AboutWindowViewModel.Library("zlib", "zlib", "Compression library", "https://zlib.net/"));
            model.Libraries.Add(new AboutWindowViewModel.Library("openssl", "OpenSSL and SSLeay", "Cryptographic library", "https://www.openssl.org/"));
            model.Libraries.Add(new AboutWindowViewModel.Library("boost", "Boost Software", "Cross-platform C++ library", "https://www.boost.org/"));
            model.Libraries.Add(new AboutWindowViewModel.Library("nlohmann/json", "MIT", "JSON library", "https://github.com/nlohmann/json"));
            model.Libraries.Add(new AboutWindowViewModel.Library("Crc32.NET", "MIT", "CRC32 library", "https://github.com/force-net/Crc32.NET"));
            model.Libraries.Add(new AboutWindowViewModel.Library("BouncyCastle.Cryptography", "MIT", "Cryptography API library", "https://www.bouncycastle.org/"));
        }
    }
}
