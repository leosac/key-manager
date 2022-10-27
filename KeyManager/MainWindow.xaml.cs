using Leosac.KeyManager.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Leosac.KeyManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel(MainSnackbar.MessageQueue!);
        }

        private void MenuDarkModeButton_Click(object sender, RoutedEventArgs e)
            => ModifyTheme(DarkModeToggleButton.IsChecked == true);

        private void OnSelectedMenuItemChanged(object sender, DependencyPropertyChangedEventArgs e)
            => MainScrollViewer.ScrollToHome();

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //until we had a StaysOpen flag to Drawer, this will help with scroll bars
            var dependencyObject = Mouse.Captured as DependencyObject;

            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar) return;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            MenuToggleButton.IsChecked = false;
        }

        private static void ModifyTheme(bool isDarkTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);
        }
    }
}
