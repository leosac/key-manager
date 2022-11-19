using Leosac.KeyManager.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager
{
    public class KMSettings : KMPermanentConfig<KMSettings>
    {
        public static string DefaultFileName { get => "Settings.json"; }

        private static object _objlock = new object();
        private static KMSettings? _singleton;

        public static KMSettings GetSingletonInstance(bool forceRecreate = false)
        {
            lock (_objlock)
            {
                if (_singleton == null || forceRecreate)
                {
                    _singleton = LoadFromFile();
                }

                return _singleton!;
            }
        }

        public static KMSettings? LoadFromFile()
        {
            return LoadFromFile(GetConfigFilePath(DefaultFileName));
        }

        public KMSettings()
        {
            UseDarkTheme = false;
            IsAutoUpdateEnabled = true;
        }

        public override string GetDefaultFileName()
        {
            return DefaultFileName;
        }

        public bool UseDarkTheme { get; set; }

        public bool IsAutoUpdateEnabled { get; set; }

        public static FileVersionInfo? GetFileVersionInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (assembly != null)
            {
                return FileVersionInfo.GetVersionInfo(assembly.Location);
            }

            return null;
        }
    }
}
