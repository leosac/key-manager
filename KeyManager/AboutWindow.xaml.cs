using Leosac.KeyManager.Domain;
using System;
using System.Collections.Generic;
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

            DataContext = new AboutWindowViewModel();
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
    }
}
