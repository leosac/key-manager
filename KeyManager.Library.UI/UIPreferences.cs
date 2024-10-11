using Leosac.SharedServices;
using System.Text.Json.Serialization;

namespace Leosac.KeyManager.Library.UI
{
    public class UIPreferences : PermanentConfig<UIPreferences>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        private static readonly object _objlock = new();
        private static UIPreferences? _singleton;

        public static UIPreferences? GetSingletonInstance(bool forceRecreate)
        {
            lock (_objlock)
            {
                if (_singleton == null || forceRecreate)
                {
                    _singleton = LoadFromFile(true);
                }

                return _singleton;
            }
        }

        private int _defaultChecksumAlgorithm;
        public int DefaultChecksumAlgorithm
        {
            get { return _defaultChecksumAlgorithm; }
            set { SetProperty(ref _defaultChecksumAlgorithm, value); }
        }

        [JsonIgnore]
        public static bool IsUserElevated { get; set; }
    }
}
