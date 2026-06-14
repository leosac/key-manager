using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.UI.Helpers;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyEntryDialogViewModel : ObservableValidator
    {
        public KeyEntryDialogViewModel(KeyEntryClass keClass)
        {
            KClass = keClass;
            _submitButtonText = Properties.Resources.OK;
            KeyEntryFactories = new ObservableCollection<KeyEntryItem>();
            Variants = new ObservableCollection<KeyEntryVariant>();
            ExpandKeyContainers = UIPreferences.GetSingletonInstance(false)?.ExpandKeyContainersByDefault ?? false;
            BuildFactories();
            OpenLinkCommand = new AsyncRelayCommand(OpenLinkAsync);
            BeforeSubmitCommand = new RelayCommand(() => DialogHost.CloseDialogCommand.Execute(KeyEntry, null), CanSubmit);
            ErrorsChanged += (_, _) => InvalidateSubmit();
        }

        private bool _canChangeFactory = true;
        private bool _showKeyMaterials = true;
        private bool _showKeyEntryLabel = true;
        private bool _allowSubmit = true;
        private string _submitButtonText;
        private bool _expandKeyContainers;
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
                var oldValue = _selectedFactoryItem;
                if (!SetProperty(ref _selectedFactoryItem, value) ||
                    value == null || !CanChangeFactory || oldValue?.Factory == value.Factory)
                    return;
                KeyEntry = value.Factory.TargetFactory?.CreateKeyEntry();
                RefreshVariants();
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

        public bool ExpandKeyContainers
        {
            get => _expandKeyContainers;
            private set => SetProperty(ref _expandKeyContainers, value);
        }

        private void BuildFactories()
        {
            foreach (var factory in KeyEntryUIFactory.RegisteredFactories)
            {
                if (factory.TargetFactory != null &&
                    factory.TargetFactory.KClasses.Contains(KClass))
                {
                    KeyEntryFactories.Add(new KeyEntryItem(factory));
                }
            }
        }

        private async Task OpenLinkAsync()
        {
            if (KeyEntry == null)
                return;

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

        public void RefreshVariants()
        {
            if (KeyEntry == null)
                return;
            Variants.Clear();
            foreach (var variant in KeyEntry.GetAllVariants(KClass))
                Variants.Add(variant);
            KeyEntry.Variant ??= Variants.FirstOrDefault();
        }

        private void InvalidateSubmit()
        {
            BeforeSubmitCommand.NotifyCanExecuteChanged();
        }

        private bool CanSubmit()
        {
            return AllowSubmit && !HasErrors;
        }

        public void SetKeyEntry(KeyEntry? keyEntry)
        {
            if (keyEntry == null)
            {
                KeyEntry = null;
                return;
            }
            var factory = KeyEntryUIFactory.GetFactoryFromPropertyType(keyEntry.Properties?.GetType());
            if (factory == null)
                return;
            ApplyFactory(factory);
            ApplyKeyEntry(keyEntry);
        }

        private void ApplyFactory(KeyEntryUIFactory factory) =>
            SelectedFactoryItem = KeyEntryFactories.FirstOrDefault(x => ReferenceEquals(x.Factory, factory));

        private void ApplyKeyEntry(KeyEntry keyEntry)
        {
            var variant = keyEntry.Variant;
            KeyEntry = keyEntry;
            if (variant == null)
                return;
            RefreshVariants();
            ApplyVariant(variant);
        }

        private void ApplyVariant(KeyEntryVariant variant)
        {
            var emptyv = Variants.FirstOrDefault(v => v.Name == variant.Name);
            if (emptyv != null)
                Variants.Remove(emptyv);
            Variants.Add(variant);
            KeyEntry!.Variant = variant;
        }

        public AsyncRelayCommand OpenLinkCommand { get; }

        public RelayCommand BeforeSubmitCommand { get; }
    }
}
