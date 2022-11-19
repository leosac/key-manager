using Leosac.KeyManager.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Leosac.KeyManager
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new AboutWindowViewModel();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void linkWebsite_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ps = new ProcessStartInfo("https://leosac.com/")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            MaintenancePlan.OpenRegistration();
        }

        private void btnSubscribe_Click(object sender, RoutedEventArgs e)
        {
            MaintenancePlan.OpenSubscription();
        }

        private void btnDownloadUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AboutWindowViewModel model)
            {
                model.AutoUpdate.DownloadUpdate();
            }
        }

        private void btnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AboutWindowViewModel model)
            {
                model.AutoUpdate.CheckUpdate();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AutoUpdateToggleButton.IsChecked = true;
            if (KMSettings.GetSingletonInstance().IsAutoUpdateEnabled)
            {
                CheckUpdate();
            }
        }

        private void AutoUpdateToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = KMSettings.GetSingletonInstance();
            settings.IsAutoUpdateEnabled = AutoUpdateToggleButton.IsChecked.GetValueOrDefault(false);
            if (settings.IsAutoUpdateEnabled)
            {
                CheckUpdate();
            }
            settings.SaveToFile();
        }

        private void CheckUpdate()
        {
            if (DataContext is AboutWindowViewModel model)
            {
                btnCheckUpdate.IsEnabled = false;
                model.AutoUpdate.CheckUpdate();
                btnCheckUpdate.IsEnabled = true;
            }
        }
    }
}
