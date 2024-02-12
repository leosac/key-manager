using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyEntryDialogViewModel : ObservableValidator
    {
        public KeyEntryDialogViewModel(KeyEntryClass keClass)
        {
            KClass = keClass;
            _submitButtonText = Leosac.KeyManager.Library.UI.Properties.Resources.OK;
            KeyEntryFactories = new ObservableCollection<KeyEntryItem>();
            foreach (var factory in KeyEntryUIFactory.RegisteredFactories)
            {
                if (factory.TargetFactory != null && factory.TargetFactory.KClasses.Contains(KClass))
                {
                    KeyEntryFactories.Add(new KeyEntryItem(factory));
                }
            }
            Variants = new ObservableCollection<KeyEntryVariant>();

            OpenLinkCommand = new AsyncRelayCommand(async
                () =>
                {
                    if (KeyEntry != null)
                    {
                        var model = new KeyEntryLinkDialogViewModel
                        {
                            Link = KeyEntry.Link,
                            Class = KeyEntry.KClass
                        };
                        var dialog = new KeyEntryLinkDialog
                        {
                            DataContext = model
                        };

                        await DialogHelper.ForceShow(dialog, "KeyEntryDialog");
                    }
                });
            BeforeSubmitCommand = new RelayCommand(
                () =>
                {
                    DialogHost.CloseDialogCommand.Execute(KeyEntry, null);
                }, CanSubmit);
        }

        private bool _canChangeFactory = true;
        private bool _showKeyMaterials = true;
        private bool _showKeyEntryLabel = true;
        private bool _allowSubmit = true;
        private string _submitButtonText;
        private KeyEntry? _keyEntry;
        private KeyEntryItem? _selectedFactoryItem;

        public ObservableCollection<KeyEntryItem> KeyEntryFactories { get; }

        public ObservableCollection<KeyEntryVariant> Variants { get; set; }

        public KeyEntryClass KClass { get; }

        public KeyEntry? KeyEntry
        {
            get => _keyEntry;
            protected set
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
                if (value != null && value?.GetType() != KeyEntry?.GetType() && CanChangeFactory)
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

        public bool ShowKeyMaterials
        {
            get => _showKeyMaterials;
            set => SetProperty(ref _showKeyMaterials, value);
        }

        public bool ShowKeyEntryLabel
        {
            get => _showKeyEntryLabel;
            set => SetProperty(ref _showKeyEntryLabel, value);
        }

        public bool AllowSubmit
        {
            get => _allowSubmit;
            set => SetProperty(ref _allowSubmit, value);
        }

        public string SubmitButtonText
        {
            get => _submitButtonText;
            set => SetProperty(ref _submitButtonText, value);
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

        private bool CanSubmit()
        {
            return AllowSubmit && !HasErrors;
        }

        public void SetKeyEntry(KeyEntry? keyEntry)
        {
            KeyEntry = keyEntry;
            if (keyEntry != null)
            {
                var factory = KeyEntryUIFactory.GetFactoryFromPropertyType(KeyEntry!.Properties?.GetType());
                if (factory != null)
                {
                    var variant = KeyEntry.Variant;
                    SelectedFactoryItem = KeyEntryFactories.Where(item => item.Factory == factory).FirstOrDefault();
                    SelectedFactoryItem!.DataContext!.Properties = KeyEntry.Properties;
                    if (variant != null)
                    {
                        RefreshVariants();
                        var emptyv = Variants.Where(v => v.Name == variant.Name).FirstOrDefault();
                        if (emptyv != null)
                        {
                            Variants.Remove(emptyv);
                        }
                        Variants.Add(variant);
                        KeyEntry.Variant = variant;
                    }
                }
            }
        }

        public AsyncRelayCommand OpenLinkCommand { get; }

        public RelayCommand BeforeSubmitCommand { get; }
    }
}
