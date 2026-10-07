using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
using System.Globalization;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyEntriesControl.xaml
    /// </summary>
    public partial class KeyEntriesControl : UserControl
    {
        public static readonly Duration KeyEntryFadeOutAnimationDuration = LoadKeyEntryFadeOutAnimationDuration();

        public KeyEntriesControl()
        {
            InitializeComponent();
        }

        public KeyEntriesControlViewModel? KeyEntriesDataContext => DataContext as KeyEntriesControlViewModel;

        private static Duration LoadKeyEntryFadeOutAnimationDuration()
        {
            var configuredDuration = UIPreferences.GetSingletonInstance(false)?.KeyEntryFadeOutDuration;
            if (TimeSpan.TryParse(configuredDuration, CultureInfo.InvariantCulture, out var duration))
                return new Duration(duration);

            return new Duration(TimeSpan.FromSeconds(4));
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                (DataContext as KeyEntriesControlViewModel)?.RefreshKeyEntries();
            }
        }

        private void KeyEntryDeletion_OnDialogClosed(object sender, DialogClosedEventArgs e)
        {
            if (KeyEntriesDataContext == null)
                return;

            if (e.Parameter is null)
                return;

            KeyEntryDeletionHandler.Handle(KeyEntriesDataContext, e.Parameter);
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            SnackbarHelper.HandlePreviewMouseWheel(sender, e);
        }
    }
}