using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.Plugin.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyEntriesHeaderViewModel : ObservableObject
    {

        public KeyStore.KeyStore? KeyStore => Current?.KeyStore;
        public ObservableCollection<SelectableKeyEntryId>? Identifiers => Current?.Identifiers;
        public ObservableCollection<WizardFactory>? WizardFactories => Current?.WizardFactories;

        public bool ShowSelection
        {
            get => Current?.ShowSelection ?? false;
            set { if (Current != null) Current.ShowSelection = value; }
        }

        public string? SearchTerms
        {
            get => Current?.SearchTerms;
            set { if (Current != null) Current.SearchTerms = value; }
        }

        public int SelectedIdentifiersCount => Current?.SelectedIdentifiersCount ?? 0;

        public int VisibleIdentifiersCount => Current?.VisibleIdentifiersCount ?? 0;

        private KeyEntriesControlViewModel? _current;

        public KeyEntriesControlViewModel? Current
        {
            get => _current;
            set
            {
                if (ReferenceEquals(_current, value))
                    return;
                if (_current != null)
                    _current.PropertyChanged -= Current_PropertyChanged;
                if (SetProperty(ref _current, value))
                {
                    if (_current != null)
                        _current.PropertyChanged += Current_PropertyChanged;
                    NotifyAllForwardedProperties();
                }
            }
        }

        public ICommand? CreateKeyEntryCommand => Current?.CreateKeyEntryCommand;
        public ICommand? GenerateKeyEntryCommand => Current?.GenerateKeyEntryCommand;
        public ICommand? EditDefaultKeyEntryCommand => Current?.EditDefaultKeyEntryCommand;
        public ICommand? ImportCryptogramCommand => Current?.ImportCryptogramCommand;
        public ICommand? WizardCommand => Current?.WizardCommand;
        public ICommand? ShowSelectionChangedCommand => Current?.ShowSelectionChangedCommand;
        public ICommand? ToggleSelectionCommand => Current?.ToggleSelectionCommand;
        public ICommand? ToggleSelectVisibleCommand => Current?.ToggleSelectVisibleCommand;
        public ICommand? PrintSelectionCommand => Current?.PrintSelectionCommand;
        public ICommand? OrderingCommand => Current?.OrderingCommand;
        public ICommand? SearchCommand => Current?.SearchCommand;
        public ICommand? DeleteKeyEntryCommand => Current?.DeleteKeyEntryCommand;

        private void NotifyAllForwardedProperties()
        {
            if (_current is null)
                return;

            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(_current))
            {
                OnPropertyChanged(prop.Name);
            }
        }

        private static readonly HashSet<string> Forwarded = new() {
            nameof(KeyEntriesControlViewModel.KeyStore),
            nameof(KeyEntriesControlViewModel.SearchTerms),
            nameof(KeyEntriesControlViewModel.ShowSelection),
            nameof(KeyEntriesControlViewModel.SelectedIdentifiersCount),
            nameof(KeyEntriesControlViewModel.VisibleIdentifiersCount)
        };

        private void Current_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName))
            {
                NotifyAllForwardedProperties();
                return;
            }

            if (Forwarded.Contains(e.PropertyName))
                OnPropertyChanged(e.PropertyName);
        }

    }
}