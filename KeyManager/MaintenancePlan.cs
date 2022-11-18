using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager
{
    public class MaintenancePlan
    {
        public static bool HasActivePlan()
        {
            return false;
        }

        public static void OpenSubscription()
        {
            var ps = new ProcessStartInfo("https://leosac.com/key-manager/")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }

        public static void OpenRegistration()
        {
            var dialog = new PlanRegistrationWindow();
            dialog.ShowDialog();
        }
    }
}
