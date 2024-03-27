using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain
{
    public class LLAReaderViewModel
    {
        private readonly LibLogicalAccess.LibraryManager _lla;

        public LLAReaderViewModel()
        {
            _lla = LibLogicalAccess.LibraryManager.getInstance();
            ReaderProviders = new ObservableCollection<string>(_lla.getAvailableReaders().ToArray());
            ReaderUnits = new ObservableCollection<string>();
        }

        public ObservableCollection<string> ReaderProviders { get; set; }

        public ObservableCollection<string> ReaderUnits { get; set; }

        public void RefreshReaderList(LLAReaderControl config)
        {
            var prevru = config!.ReaderUnit;
            ReaderUnits.Clear();
            var rp = _lla.getReaderProvider(config.ReaderProvider);
            var ruList = rp.getReaderList();
            foreach (var ru in ruList)
            {
                ReaderUnits.Add(ru.getName());
            }
            if (ReaderUnits.Contains(prevru))
            {
                config.ReaderUnit = prevru;
            }
        }
    }
}
