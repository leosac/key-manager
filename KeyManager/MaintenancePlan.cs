using Leosac.KeyManager.Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
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
                    _singleton?.ParseCode(_singleton.Code);
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

        public string? LicenseKey { get; set; }

        [JsonIgnore]
        public DateTime? ExpirationDate { get; set; }

        public string? Code { get; set; }

        [JsonIgnore]
        public EventHandler? PlanUpdated;

        public void OnPlanUpdated()
        {
            if (PlanUpdated != null)
            {
                PlanUpdated(this, new EventArgs());
            }
        }

        public override void SaveToFile()
        {
            base.SaveToFile();
            OnPlanUpdated();
        }

        public bool IsActivePlan()
        {
            if (!string.IsNullOrEmpty(LicenseKey))
            {
                return (ExpirationDate == null || ExpirationDate.Value >= DateTime.Now);
            }
            return false;
        }

        private JToken? QueryData(string url)
        {
            var client = new HttpClient();
            var req = client.GetStringAsync(url);
            var result = req.Result;
            if (string.IsNullOrEmpty(result))
            {
                var error = "The request failed without details.";
                log.Error(error);
                throw new Exception(error);
            }

            var jobj = JObject.Parse(result);
            var success = (bool?)jobj["success"];
            if (!success.GetValueOrDefault(false))
            {
                var error = String.Format("The request failed with error: {0}.", (string?)jobj["data"]?["error"]);
                log.Error(error);
                throw new Exception(error);
            }

            return jobj["data"];
        }

        public void RegisterPlan(string licenseKey, string? email = null, string? code = null)
        {
            if (string.IsNullOrEmpty(licenseKey))
                throw new Exception("License/Plan key is required.");

            var oldKey = LicenseKey;
            if (string.IsNullOrEmpty(code))
            {
                var fragments = licenseKey.Split('-');
                var data = QueryData(String.Format("{0}?wc-api=serial-numbers-api&request=check&product_id={1}&serial_key={2}", BASE_URL, fragments[0], licenseKey));
                DateTime? expire = null;
                if (data?["expire_date"] != null)
                    expire = DateTime.Parse((string)data["expire_date"]!);
                data = QueryData(String.Format("{0}?wc-api=serial-numbers-api&request=activate&product_id={1}&serial_key={2}&instance={3}&email={4}&platform={5}", BASE_URL, fragments[0], licenseKey, GetUUID(), email, Environment.OSVersion));

                var msg = (string?)(data?["message"]);
                log.Info(String.Format("Registration succeeded with message: {0}.", msg));

                LicenseKey = licenseKey;
                ExpirationDate = expire;
                Code = ComputeCode();

                SaveToFile();
            }
            else
            {
                LicenseKey = licenseKey;
                if (ParseCode(code))
                {
                    SaveToFile();
                }
                else
                {
                    LicenseKey = oldKey;
                    var error = "Invalid verification code.";
                    log.Error(error);
                    throw new Exception(error);
                }
            }
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

        public bool ParseCode(string? code)
        {
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
                            Code = code;
                            return true;
                        }
                        else
                        {
                            LicenseKey = null;
                        }
                    }
                }
            }

            return false;
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
            var ps = new ProcessStartInfo(String.Format("https://leosac.com/key-manager/"))
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

        public static string GetPlanUrl(string key)
        {
            return String.Format("{0}/show-plan?serial_key={1}", BASE_URL, key);
        }

        public static string GetOfflineRegistrationUrl(string? key, string? uuid)
        {
            var url = String.Format("{0}register-plan?", BASE_URL);
            if (!string.IsNullOrEmpty(key))
                url += String.Format("serial_key={0}&", key);
            if (!string.IsNullOrEmpty(uuid))
                url += String.Format("instance={0}", uuid);
            return url;
        }
    }
}
