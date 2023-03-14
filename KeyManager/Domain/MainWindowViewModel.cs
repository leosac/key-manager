using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using Leosac.KeyManager.Library.KeyStore;
using System.Windows;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.Plugin.Domain;
using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Domain
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public MainWindowViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            log.Debug("Initializing KeyManager MainWindow view model...");

            HomeCommand = new KeyManagerCommand(
                _ =>
                {
                    SelectedIndex = 0;
                });
            FavoritesCommand = new KeyManagerCommand(
                _ =>
                {
                    SelectedIndex = 1;
                });
            KeyStoreCommand = new KeyManagerCommand(
                parameter =>
                {
                    if (parameter != null)
                    {
                        KeyStore? ks = null;
                        Favorite? fav = null;
                        KeyStoreFactory? factory = null;
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
                            factory = KeyStoreFactory.GetFactoryFromPropertyType(ks.Properties!.GetType());
                            if (factory != null)
                            {
                                try
                                {
                                    SelectedIndex = 2;
                                    var editModel = _selectedItem?.DataContext as EditKeyStoreControlViewModel;
                                    if (editModel != null)
                                    {
                                        // Ensure everything is back to original state
                                        editModel.CloseKeyStore(false);
                                        editModel.KeyStore = ks;
                                        editModel.Favorite = fav;
                                        editModel.OpenKeyStore();

                                        var additionalControls = factory.CreateKeyStoreAdditionalControls();
                                        foreach (var addition in additionalControls)
                                        {
                                            if (addition.Value.DataContext is KeyStoreAdditionalControlViewModel additionalModel)
                                            {
                                                additionalModel.KeyStore = ks;
                                                additionalModel.SnackbarMessageQueue = snackbarMessageQueue;
                                            }
                                            editModel.Tabs.Add(new TabItem() { Header = addition.Key, Content = addition.Value });
                                        }
                                    }
                                }
                                catch (KeyStoreException ex)
                                {
                                    SnackbarHelper.EnqueueError(snackbarMessageQueue, ex, "Key Store Error");
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Opening Key Store failed unexpected.", ex);
                                    SnackbarHelper.EnqueueError(snackbarMessageQueue, ex);
                                }
                            }
                        }
                    }

                });
            LogConsoleCommand = new KeyManagerCommand(
                parameter =>
                {
                    var consoleWindow = new LogConsoleWindow();
                    consoleWindow.Show();
                });
            OpenAboutCommand = new KeyManagerCommand(
                parameter =>
                {
                    var aboutWindow = new AboutWindow();
                    aboutWindow.ShowDialog();
                });
            ChangeLanguageCommand = new KeyManagerCommand(
                parameter =>
                {
                    var lang = "en-US";
                    if (parameter != null)
                    {
                        lang = parameter.ToString();
                    }

                    var settings = KMSettings.GetSingletonInstance();
                    settings.Language = lang;
                    settings.SaveToFile();

                    LangHelper.ChangeLanguage(lang);

                    // Restart the application to be sure already created Windows have expected language
                    var module = Process.GetCurrentProcess().MainModule;
                    if (module != null && !string.IsNullOrEmpty(module.FileName))
                    {
                        System.Diagnostics.Process.Start(module.FileName);
                    }
                    Application.Current.Shutdown();
                });

            MenuItems = new ObservableCollection<NavItem>(new[]
            {
                new NavItem(
                    Properties.Resources.MenuHome,
                    typeof(HomeControl),
                    "House",
                    new HomeControlViewModel(snackbarMessageQueue)
                    {
                        KeyStoreCommand = KeyStoreCommand,
                        FavoritesCommand = FavoritesCommand
                    }
                ),
                new NavItem(
                    Properties.Resources.MenuFavorites,
                    typeof(FavoritesControl),
                    "Star",
                    new FavoritesControlViewModel(snackbarMessageQueue)
                    {
                        KeyStoreCommand = KeyStoreCommand
                    }
                ),
                new NavItem(
                    Properties.Resources.MenuKeyStore,
                    typeof(EditKeyStoreControl),
                    "ShieldKeyOutline",
                    new EditKeyStoreControlViewModel(snackbarMessageQueue)
                    {
                        HomeCommand = HomeCommand
                    }
                )
            });
            SelectedItem = MenuItems[0];
            SelectedIndex = 0;

            _snackbarMessageQueue = snackbarMessageQueue;
            _navItemsView = CollectionViewSource.GetDefaultView(MenuItems);
            var plan = MaintenancePlan.GetSingletonInstance();
            _showPlanFooter = !plan.IsActivePlan();
            plan.PlanUpdated += (sender, e) => { ShowPlanFooter = !plan.IsActivePlan(); };
        }

        private ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly ICollectionView _navItemsView;
        private NavItem? _selectedItem;
        private int _selectedIndex;
        private bool _showPlanFooter;
        private bool _isDarkMode;

        public ObservableCollection<NavItem> MenuItems { get; }

        public NavItem? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        public bool ShowPlanFooter
        {
            get => _showPlanFooter;
            set => SetProperty(ref _showPlanFooter, value);
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                SetProperty(ref _isDarkMode, value);
                ModifyAndSaveTheme(value);
            }
        }

        public KeyManagerCommand HomeCommand { get; }
        public KeyManagerCommand FavoritesCommand { get; }
        public KeyManagerCommand KeyStoreCommand { get; }
        public KeyManagerCommand LogConsoleCommand { get; }
        public KeyManagerCommand OpenAboutCommand { get; }
        public KeyManagerCommand ChangeLanguageCommand { get; }

        private static void ModifyTheme(bool isDarkTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);
        }

        public void ModifyAndSaveTheme(bool isDarkTheme)
        {
            ModifyTheme(isDarkTheme);

            var settings = KMSettings.GetSingletonInstance();
            settings.UseDarkTheme = isDarkTheme;
            settings.SaveToFile();
        }

        public void InitFromSettings()
        {
            var settings = KMSettings.GetSingletonInstance();
            if (settings.UseDarkTheme)
            {
                IsDarkMode = settings.UseDarkTheme;
            }
            if (settings.IsAutoUpdateEnabled)
            {
                var update = new AutoUpdate();
                if (update.CheckUpdate())
                {
                    var wrapControl = new WrapPanel();
                    wrapControl.Orientation = Orientation.Horizontal;
                    wrapControl.Margin = new Thickness(5);
                    wrapControl.HorizontalAlignment = HorizontalAlignment.Center;
                    wrapControl.VerticalAlignment = VerticalAlignment.Center;
                    var textControl = new TextBlock();
                    textControl.Text = Properties.Resources.NewUpdateAvailable;
                    wrapControl.Children.Add(textControl);
                    var buttonControl = new Button();
                    buttonControl.Content = Properties.Resources.DownloadNow;
                    buttonControl.Click += (sender, e) => { update.DownloadUpdate(); };
                    buttonControl.Style = Application.Current.FindResource("MaterialDesignFlatButton") as Style;
                    buttonControl.Margin = new Thickness(20, 0, 0, 0);
                    wrapControl.Children.Add(buttonControl);

                    _snackbarMessageQueue.Enqueue(wrapControl, null, null, null, false, true, TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}
