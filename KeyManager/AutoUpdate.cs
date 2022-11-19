using Leosac.KeyManager.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager
{
    public class AutoUpdate : KMObject
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public AutoUpdate()
        {
            _hasUpdate = false;
        }

        private bool _hasUpdate;
        private UpdateVersion? _updateVersion;

        public bool CheckUpdate()
        {
            log.Info("Checking for software update...");

            try
            {
                var client = new HttpClient();
                using (var response = client.GetAsync("https://raw.githubusercontent.com/leosac/key-manager/master/latestversion").Result)
                {
                    using (var content = response.Content)
                    {
                        var json = content.ReadAsStringAsync().Result;
                        UpdateVersion = JsonConvert.DeserializeObject<UpdateVersion>(json);

                        if (UpdateVersion != null)
                        {
                            var fvi = KMSettings.GetFileVersionInfo();

                            if (!string.IsNullOrEmpty(fvi?.ProductVersion) && !string.IsNullOrEmpty(UpdateVersion.VersionString))
                            {
                                var currentVersion = new Version(fvi.ProductVersion);
                                var newVersion = new Version(UpdateVersion.VersionString);

                                if (newVersion > currentVersion)
                                {
                                    log.Info("New update available!");
                                    HasUpdate = true;
                                }
                                else
                                {
                                    log.Info("There is no update available.");
                                    HasUpdate = false;
                                }
                            }
                            else
                            {
                                log.Error("Cannot retrieve software versions.");
                            }
                        }
                        else
                        {
                            log.Error("Cannot unserialize the software update.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Software update check failed.", ex);
                HasUpdate = false;
            }

            return HasUpdate;
        }

        public bool HasUpdate
        {
            get => _hasUpdate;
            set => SetProperty(ref _hasUpdate, value);
        }

        public UpdateVersion? UpdateVersion
        {
            get => _updateVersion;
            set => SetProperty(ref _updateVersion, value);
        }

        public void DownloadUpdate()
        {
            if (UpdateVersion != null)
            {
                var ps = new ProcessStartInfo(UpdateVersion.Uri)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);
            }
        }
    }
}
