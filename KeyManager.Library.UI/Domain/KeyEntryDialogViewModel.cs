using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyEntryDialogViewModel : ViewModelBase
    {
        public KeyEntryDialogViewModel()
        {
            KeyEntryFactories = new ObservableCollection<KeyEntryItem>();
            foreach (var factory in KeyEntryUIFactory.RegisteredFactories)
            {
                if (factory.TargetFactory != null && factory.TargetFactory.KClasses.Contains(KClass))
                {
                    KeyEntryFactories.Add(new KeyEntryItem(factory));
                }
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
            BeforeSubmitCommand = new KeyManagerCommand(
                parameter =>
                {

                }, CanSubmit);
        }

        private bool _canChangeFactory = true;
        private bool _autoCreate = true;
        private KeyEntryClass _kClass = KeyEntryClass.Symmetric;
        private KeyEntry? _keyEntry;
        private KeyEntryItem? _selectedFactoryItem;

        public ObservableCollection<KeyEntryItem> KeyEntryFactories { get; }

        public ObservableCollection<KeyEntryVariant> Variants { get; set; }

        public KeyEntryClass KClass
        {
            get => _kClass;
            set => SetProperty(ref _kClass, value);
        }

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
                    KeyEntry = _selectedFactoryItem?.Factory.TargetFactory?.CreateKeyEntry();
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
                var variants = KeyEntry.GetAllVariants(KClass);
                foreach (var variant in variants)
                {
                    Variants.Add(variant);
                }

                if (KeyEntry.Variant == null)
                {
                    KeyEntry.Variant = Variants.FirstOrDefault();
                }
            }
        }

        private bool CanSubmit(object? parameter)
        {
            return !HasErrors;
        }

        public KeyManagerAsyncCommand<object> OpenLinkCommand { get; }

        public KeyManagerCommand BeforeSubmitCommand { get; }
    }
}
