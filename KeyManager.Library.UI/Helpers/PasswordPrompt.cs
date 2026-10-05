using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI.Helpers
{
    public static class PasswordPrompt
    {
        public static string? Show(string title)
        {
            var passwordBox = new PasswordBox
            {
                MinWidth = 280,
                Margin = new Thickness(0, 8, 0, 16)
            };
            var panel = new StackPanel
            {
                Margin = new Thickness(20)
            };
            panel.Children.Add(new TextBlock { Text = title });
            panel.Children.Add(passwordBox);

            var window = new Window
            {
                Content = panel,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current?.MainWindow,
                Title = title
            };
            var button = new Button
            {
                Content = "OK",
                IsDefault = true,
                Padding = new Thickness(16, 4, 16, 4),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            button.Click += (_, _) => window.DialogResult = true;
            panel.Children.Add(button);

            return window.ShowDialog() == true ? passwordBox.Password : null;
        }
    }
}
