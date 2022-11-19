using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf.Transitions;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Leosac.KeyManager.Library.KeyStore;
using System.Windows;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library;

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

                                        while (editModel.Tabs.Count > 1)
                                        {
                                            editModel.Tabs.RemoveAt(1);
                                        }
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

                                        ks?.Open();
                                        editModel.KeyStore = ks;
                                        editModel.Favorite = fav;
                                        editModel.RefreshKeyEntries();
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

            MenuItems = new ObservableCollection<NavItem>(new[]
            {
                new NavItem(
                    "Home",
                    typeof(HomeControl),
                    "House",
                    new HomeControlViewModel(snackbarMessageQueue)
                    {
                        KeyStoreCommand = KeyStoreCommand,
                        FavoritesCommand = FavoritesCommand
                    }
                ),
                new NavItem(
                    "Favorites",
                    typeof(FavoritesControl),
                    "Star",
                    new FavoritesControlViewModel(snackbarMessageQueue)
                    {
                        KeyStoreCommand = KeyStoreCommand
                    }
                ),
                new NavItem(
                    "Current Key Store",
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
            var plan = MaintenancePlan.getSingleton();
            _showPlanFooter = !plan.HasActivePlan();
        }

        private ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly ICollectionView _navItemsView;
        private NavItem? _selectedItem;
        private int _selectedIndex;
        private bool _showPlanFooter;

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

        public KeyManagerCommand HomeCommand { get; }
        public KeyManagerCommand FavoritesCommand { get; }
        public KeyManagerCommand KeyStoreCommand { get; }
        public KeyManagerCommand LogConsoleCommand { get; }
        public KeyManagerCommand OpenAboutCommand { get; }

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
                ModifyTheme(settings.UseDarkTheme);
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
                    textControl.Text = "New software update available!";
                    wrapControl.Children.Add(textControl);
                    var buttonControl = new Button();
                    buttonControl.Content = "Download now";
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
