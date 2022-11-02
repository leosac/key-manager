using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain
{
    public class SAMKeyEntryPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public SAMKeyEntryPropertiesControlViewModel()
        {
            _properties = new SAMKeyEntryProperties();
            SAMKeyEntryTypes = new ObservableCollection<SAMKeyEntryType>(Enum.GetValues<SAMKeyEntryType>());
        }

        public ObservableCollection<SAMKeyEntryType> SAMKeyEntryTypes { get; set; }

        public SAMKeyEntryProperties? SAMProperties
        {
            get { return Properties as SAMKeyEntryProperties; }
        }
    }
}
