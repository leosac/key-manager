using Leosac.KeyManager.Library.UI;
using Microsoft.Win32;
using System;
using System.Windows;

namespace Leosac.KeyManager
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is KMSettings settings)
            {
                if (!settings.SaveToFile())
                {
                    MessageBox.Show(Properties.Resources.SaveConfigFileError, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "JSON files (*.json)|*.json";
            ofd.CheckFileExists = false;
            if (ofd.ShowDialog() == true && DataContext is KMSettings settings)
            {
                settings.FavoritesPath = ofd.FileName;
            }
        }
    }
}
