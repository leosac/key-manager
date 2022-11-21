using Leosac.KeyManager.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace Leosac.KeyManager
{
    /// <summary>
    /// Maintenance plan.
    /// </summary>
    /// <remarks>
    /// This is not a security feature neither a proper licensing feature. You are free to remove this code and recompile the application. A generator could also easily be created.
    /// It is mainly designed to help Leosac company to manage the commercial plans provided to customers and the appropriate registered support services.
    /// </remarks>
    public class MaintenancePlan : KMPermanentConfig<MaintenancePlan>
    {
        const string BASE_URL = "https://leosac.com/";

        public static string DefaultFileName { get => "MaintenancePlan.json"; }

        private static object _objlock = new object();
        private static MaintenancePlan? _singleton;

        public static MaintenancePlan GetSingletonInstance(bool forceRecreate = false)
        {
            lock (_objlock)
            {
                if (_singleton == null || forceRecreate)
                {
                    _singleton = LoadFromFile();
                    _singleton?.ParseCode();
                }

                return _singleton!;
            }
        }

        public static MaintenancePlan? LoadFromFile()
        {
            return LoadFromFile(GetConfigFilePath(DefaultFileName));
        }

        public MaintenancePlan()
        {

        }

        public override string GetDefaultFileName()
        {
            return DefaultFileName;
        }

        [JsonIgnore]
        public string? LicenseKey { get; set; }

        [JsonIgnore]
        public DateTime? ExpirationDate { get; set; }

        public string? Code { get; set; }

        public bool IsActivePlan()
        {
            if (!string.IsNullOrEmpty(LicenseKey))
            {
                return (ExpirationDate == null || ExpirationDate.Value >= DateTime.Now);
            }
            return false;
        }

        public void RegisterPlan(string licenseKey, string? code = null)
        {

        }

        public string? ComputeCode()
        {
            if (!string.IsNullOrEmpty(LicenseKey))
            {
                var expiration = string.Empty;
                if (ExpirationDate != null)
                {
                    expiration = ExpirationDate.Value.ToString("MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
                var key = GetUUIDKey();
                if (key != null)
                {
                    var aes = Aes.Create();
                    var iv = new byte[16];
                    aes.Key = key;
                    return Convert.ToHexString(aes.EncryptCbc(Encoding.UTF8.GetBytes(String.Format("LEO:{0}:{1}", LicenseKey, expiration)), iv));
                }
            }

            return null;
        }

        public void ParseCode(string? code = null)
        {
            if (string.IsNullOrEmpty(code))
                code = Code;

            if (!string.IsNullOrEmpty(code))
            {
                var key = GetUUIDKey();
                if (key != null)
                {
                    var aes = Aes.Create();
                    var iv = new byte[16];
                    aes.Key = key;
                    var chain = Encoding.UTF8.GetString(aes.DecryptCbc(Convert.FromHexString(code), iv));
                    var fragments = chain.Split(':');
                    if (fragments.Length == 3 && fragments[0] == "LEO")
                    {
                        if (LicenseKey == fragments[1])
                        {
                            ExpirationDate = DateTime.ParseExact(fragments[2], "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            LicenseKey = null;
                        }
                    }
                }
            }
        }

        private byte[]? GetUUIDKey()
        {
            var uuid = GetUUID();
            if (!string.IsNullOrEmpty(uuid))
            {
                var deriv = new Rfc2898DeriveBytes(uuid, Encoding.UTF8.GetBytes("Security Freedom"));
                return deriv.GetBytes(16);
            }

            return null;
        }

        public string? GetUUID()
        {
            string? uuid = null;
            var settings = KMSettings.GetSingletonInstance();
            if (settings.InstallationId.Length > 12)
            {
                var hash = MD5.Create();
                uuid = Convert.ToHexString(hash.ComputeHash(Encoding.UTF8.GetBytes(String.Format("{0}:{1}", settings.InstallationId, Environment.MachineName))));
            }
            return uuid;
        }

        public static void OpenSubscription()
        {
            var settings = KMSettings.GetSingletonInstance();
            var ps = new ProcessStartInfo(String.Format("https://leosac.com/key-manager/?installationId={0}", settings.InstallationId))
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);

            // Ensure we save the InstallationId being used if generated for the first time
            settings.SaveToFile();
        }

        public static void OpenRegistration()
        {
            var dialog = new PlanRegistrationWindow();
            dialog.ShowDialog();
        }

        public static string GetPlanUrl(string key)
        {
            return String.Format("{0}/show-plan?key={1}", BASE_URL, key);
        }

        public static string GetOfflineRegistrationUrl(string? key, string? uuid)
        {
            var url = String.Format("{0}register-plan?", BASE_URL);
            if (!string.IsNullOrEmpty(key))
                url += String.Format("key={0}&", key);
            if (!string.IsNullOrEmpty(uuid))
                url += String.Format("uuid={0}", uuid);
            return url;
        }
    }
}
