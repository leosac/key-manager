using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyEntryDialogViewModel : ViewModelBase
    {
        public KeyEntryDialogViewModel()
        {
            KeyEntryFactories = new ObservableCollection<KeyEntryItem>();
            foreach (var factory in KeyEntryFactory.RegisteredFactories)
            {
                KeyEntryFactories.Add(new KeyEntryItem(factory));
            }
        }

        private bool _canChangeFactory = true;
        private KeyEntry? _keyEntry;
        private KeyEntryItem? _selectedFactoryItem;

        public ObservableCollection<KeyEntryItem> KeyEntryFactories { get; }

        public KeyEntry? KeyEntry
        {
            get => _keyEntry;
            set
            {
                SetProperty(ref _keyEntry, value);
                if (_selectedFactoryItem != null)
                {
                    if (_selectedFactoryItem?.DataContext != null)
                    {
                        _selectedFactoryItem.DataContext.Properties = _keyEntry?.Properties;
                    }
                }
            }
        }

        public KeyEntryItem? SelectedFactoryItem
        {
            get => _selectedFactoryItem;
            set => SetProperty(ref _selectedFactoryItem, value);
        }

        public bool CanChangeFactory
        {
            get => _canChangeFactory;
            set => SetProperty(ref _canChangeFactory, value);
        }
    }
}
