using System.Windows;
using System.Windows.Controls;
using Leosac.KeyManager.Library.KeyStore;

namespace Leosac.KeyManager.Library.UI
{
    public partial class KeyEntryReferenceControl : UserControl
    {
        public KeyEntryReferenceControl()
        {
            InitializeComponent();
            KeyEntryId = new KeyEntryId();
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
    }
}
