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
    /// Interaction logic for PlanRegistrationWindow.xaml
    /// </summary>
    public partial class PlanRegistrationWindow : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public PlanRegistrationWindow()
        {
            InitializeComponent();

            DataContext = new PlanRegistrationWindowViewModel();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnRegisterOffline_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PlanRegistrationWindowViewModel model && !string.IsNullOrEmpty(model.Key) && !string.IsNullOrEmpty(model.Code))
            {
                try
                {
                    var plan = MaintenancePlan.GetSingletonInstance();
                    plan.RegisterPlan(model.Key, model.Code);
                }
                catch (Exception ex)
                {
                    log.Error("Plan registration failed.", ex);
                }
            }
        }

        private void btnRegisterOnline_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PlanRegistrationWindowViewModel model && !string.IsNullOrEmpty(model.Key))
            {
                try
                {
                    var plan = MaintenancePlan.GetSingletonInstance();
                    plan.RegisterPlan(model.Key);
                }
                catch (Exception ex)
                {
                    log.Error("Plan registration failed.", ex);
                }
            }
        }

        private void OfflineRegistrationUrl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenRegistrationUrl();
        }

        private void OpenUrl_Click(object sender, RoutedEventArgs e)
        {
            OpenRegistrationUrl();
        }

        private void OpenRegistrationUrl()
        {
            if (DataContext is PlanRegistrationWindowViewModel model)
            {
                var ps = new ProcessStartInfo(model.OfflineRegistrationUrl)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);
            }
        }

        private void CopyUrl_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PlanRegistrationWindowViewModel model)
            {
                Clipboard.SetText(model.OfflineRegistrationUrl);
            }
        }
    }
}
