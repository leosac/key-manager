using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        private LibLogicalAccess.LibraryManager _lla;

        public SAMKeyStoreProperties? SAMProperties
        {
            get { return Properties as SAMKeyStoreProperties; }
        }

        public ObservableCollection<string> ReaderProviders { get; set; }

        public ObservableCollection<string> ReaderUnits { get; set; }

        public void RefreshReaderList()
        {
            var prevru = SAMProperties.ReaderUnit;
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
