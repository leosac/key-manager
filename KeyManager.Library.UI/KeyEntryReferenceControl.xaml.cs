using System.Windows;
using System.Windows.Controls;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;

namespace Leosac.KeyManager.Library.UI
{
    public partial class KeyEntryReferenceControl : UserControl
    {
        public KeyEntryReferenceControl()
        {
            InitializeComponent();
            KeyEntryId = new KeyEntryId();
            SearchDialogIdentifier = Guid.NewGuid().ToString();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new KeyEntrySearchDialog
            {
                DataContext = new KeyEntrySearchDialogViewModel(KeyStoreFavorite)
            };
            var result = await DialogHost.Show(dialog, SearchDialogIdentifier);
            if (result is KeyEntryId selected)
            {
                KeyEntryId.Id = selected.Id;
                KeyEntryId.Label = selected.Label;
            }
        }

        public KeyEntryId KeyEntryId
        {
            get => (KeyEntryId)GetValue(KeyEntryIdProperty);
            set => SetValue(KeyEntryIdProperty, value);
        }

        public static readonly DependencyProperty KeyEntryIdProperty = DependencyProperty.Register(
            nameof(KeyEntryId), typeof(KeyEntryId), typeof(KeyEntryReferenceControl), new FrameworkPropertyMetadata());

        public string? KeyStoreFavorite
        {
            get => (string?)GetValue(KeyStoreFavoriteProperty);
            set => SetValue(KeyStoreFavoriteProperty, value);
        }

        public static readonly DependencyProperty KeyStoreFavoriteProperty = DependencyProperty.Register(
            nameof(KeyStoreFavorite), typeof(string), typeof(KeyEntryReferenceControl), new FrameworkPropertyMetadata());

        public string SearchDialogIdentifier
        {
            get => (string)GetValue(SearchDialogIdentifierProperty);
            set => SetValue(SearchDialogIdentifierProperty, value);
        }

        public static readonly DependencyProperty SearchDialogIdentifierProperty = DependencyProperty.Register(
            nameof(SearchDialogIdentifier), typeof(string), typeof(KeyEntryReferenceControl), new FrameworkPropertyMetadata());
    }
}
