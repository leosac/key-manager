using Leosac.KeyManager.Library.KeyStore;
using MaterialDesignThemes.Wpf;
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
            Variants = new ObservableCollection<KeyEntryVariant>();

            OpenLinkCommand = new KeyManagerAsyncCommand<object>(async
                parameter =>
                {
                    if (KeyEntry != null)
                    {
                        var model = new KeyEntryLinkDialogViewModel()
                        {
                            Link = KeyEntry.Link,
                            Class = KeyEntry.KClass
                        };
                        var dialog = new KeyEntryLinkDialog()
                        {
                            DataContext = model
                        };

                        await DialogHost.Show(dialog, "KeyEntryDialog");
                    }
                });
        }

        private bool _canChangeFactory = true;
        private bool _autoCreate = true;
        private KeyEntry? _keyEntry;
        private KeyEntryItem? _selectedFactoryItem;

        public ObservableCollection<KeyEntryItem> KeyEntryFactories { get; }

        public ObservableCollection<KeyEntryVariant> Variants { get; set; }

        public KeyEntry? KeyEntry
        {
            get => _keyEntry;
            set
            {
                SetProperty(ref _keyEntry, value);
                if (_selectedFactoryItem?.DataContext != null)
                {
                    _selectedFactoryItem.DataContext.Properties = _keyEntry?.Properties;
                }
            }
        }

        public KeyEntryItem? SelectedFactoryItem
        {
            get => _selectedFactoryItem;
            set
            {
                SetProperty(ref _selectedFactoryItem, value);
                if (value != null && value?.GetType() != KeyEntry?.GetType() && AutoCreate)
                {
                    KeyEntry = _selectedFactoryItem?.Factory.CreateKeyEntry();
                    RefreshVariants();
                }
            }
        }

        public bool CanChangeFactory
        {
            get => _canChangeFactory;
            set => SetProperty(ref _canChangeFactory, value);
        }

        public bool AutoCreate
        {
            get => _autoCreate;
            set => SetProperty(ref _autoCreate, value);
        }

        public void RefreshVariants()
        {
            if (KeyEntry != null)
            {
                Variants.Clear();
                var variants = KeyEntry.GetAllVariants();
                foreach (var variant in variants)
                {
                    Variants.Add(variant);
                }
            }
        }

        public KeyManagerAsyncCommand<object> OpenLinkCommand { get; }
    }
}
