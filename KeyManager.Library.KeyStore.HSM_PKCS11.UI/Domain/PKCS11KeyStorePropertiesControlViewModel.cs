﻿using Leosac.KeyManager.Library.Plugin.Domain;
using Net.Pkcs11Interop.Common;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.Domain
{
    public class PKCS11KeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public PKCS11KeyStorePropertiesControlViewModel()
        {
            _properties = new PKCS11KeyStoreProperties();
            FilterTypes = new ObservableCollection<SlotFilterType>(Enum.GetValues<SlotFilterType>());
            UserTypes = new ObservableCollection<CKU>(Enum.GetValues<CKU>());
        }

        public PKCS11KeyStoreProperties? PKCS11Properties
        {
            get { return Properties as PKCS11KeyStoreProperties; }
        }

        public ObservableCollection<SlotFilterType> FilterTypes { get; private set; }

        public ObservableCollection<CKU> UserTypes { get; private set; }
    }
}
