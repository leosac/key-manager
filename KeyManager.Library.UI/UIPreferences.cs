using Leosac.SharedServices;

namespace Leosac.KeyManager.Library.UI
{
    public class UIPreferences : PermanentConfig<UIPreferences>
    {
        private int _defaultChecksumAlgorithm;
        public int DefaultChecksumAlgorithm
        {
            get { return _defaultChecksumAlgorithm; }
            set { SetProperty(ref _defaultChecksumAlgorithm, value); }
        }
    }
}
