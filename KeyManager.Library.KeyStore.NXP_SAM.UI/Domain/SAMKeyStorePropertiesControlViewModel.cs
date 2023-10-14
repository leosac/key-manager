using Leosac.KeyManager.Library.Plugin.UI.Domain;
using LibLogicalAccess.Card;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain
{
    public class SAMKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public SAMKeyStorePropertiesControlViewModel()
        {
            _properties = new SAMKeyStoreProperties();
            _lla = LibLogicalAccess.LibraryManager.getInstance();
            ReaderProviders = new ObservableCollection<string>(_lla.getAvailableReaders().ToArray());
            ReaderUnits = new ObservableCollection<string>();
            KeyTypes = new ObservableCollection<DESFireKeyType>(Enum.GetValues<DESFireKeyType>());
            CardTypes = new ObservableCollection<string>(new[]
            {
                "SAM_AV1",
                "SAM_AV2",
                "SAM_AV3"
            });
        }

        private readonly LibLogicalAccess.LibraryManager _lla;

        public SAMKeyStoreProperties? SAMProperties
        {
            get { return Properties as SAMKeyStoreProperties; }
        }

        public ObservableCollection<string> ReaderProviders { get; set; }

        public ObservableCollection<string> ReaderUnits { get; set; }

        public ObservableCollection<DESFireKeyType> KeyTypes { get; set; }

        public ObservableCollection<string> CardTypes { get; set; }

        public void RefreshReaderList()
        {
            var prevru = SAMProperties!.ReaderUnit;
            ReaderUnits.Clear();
            var rp = _lla.getReaderProvider(SAMProperties.ReaderProvider);
            var ruList = rp.getReaderList();
            foreach (var ru in ruList)
            {
                ReaderUnits.Add(ru.getName());
            }
            if (ReaderUnits.Contains(prevru))
            {
                SAMProperties.ReaderUnit = prevru;
            }
        }
    }
}
