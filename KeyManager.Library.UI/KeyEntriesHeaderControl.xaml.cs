using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyEntriesHeaderControl.xaml
    /// </summary>
    public partial class KeyEntriesHeaderControl : UserControl
    {

        private ScrollViewer? _scrollViewer;

        public KeyEntriesHeaderControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private KeyEntriesControlViewModel? ResolveVm() =>
            DataContext switch
            {
                KeyEntriesControlViewModel vm => vm,
                KeyEntriesHeaderViewModel header => header.Current,
                _ => null
            };

        private void KeyEntryDeletion_OnDialogClosed(object sender, DialogClosedEventArgs e)
        {
            var vm = ResolveVm();
            if (vm == null)
                return;

            if (e.Parameter is null)
                return;

            KeyEntryDeletionHandler.Handle(vm, e.Parameter);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _scrollViewer = FindScrollViewer(this);

            if (_scrollViewer == null)
                return;

            _scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_scrollViewer != null)
                _scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;

            _scrollViewer = null;
        }

        private ScrollViewer? FindScrollViewer(DependencyObject? element)
        {
            while (element != null)
            {
                if (element is ScrollViewer sv)
                    return sv;

                element = VisualTreeHelper.GetParent(element);
            }
            return null;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_scrollViewer == null || sender != _scrollViewer)
                return;

            var vm = ResolveVm();
            if (vm == null)
                return;

            var position = TranslatePoint(new Point(0, 0), _scrollViewer);
            var collapsed = position.Y < 0;

            if (vm.IsToolbarCollapsed != collapsed)
                vm.IsToolbarCollapsed = collapsed;
        }
    }
}
