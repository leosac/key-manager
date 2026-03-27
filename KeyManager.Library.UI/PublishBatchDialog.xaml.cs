using Leosac.KeyManager.Library.KeyStore;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace Leosac.KeyManager.Library.UI
{
    public partial class PublishBatchDialog : UserControl
    {
        private ScrollViewer? _logTreeScrollViewer;
        private Domain.LogEntry? _lastParentNode;
        private bool _autoScroll = true;

        public PublishBatchDialog()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                if (LogTree != null)
                {
                    _logTreeScrollViewer = GetScrollViewer(LogTree);
                    if (_logTreeScrollViewer != null)
                        _logTreeScrollViewer.ScrollChanged += LogScroll;
                }
                if (DataContext is Domain.PublishBatchDialogViewModel vm)
                    vm.Logs.CollectionChanged += ChangedLogs;
                if (GetScrollViewer(LogTree) is ScrollViewer s)
                    s.ScrollChanged += LogScroll;
            };
            Unloaded += (_, _) =>
            {
                if (DataContext is Domain.PublishBatchDialogViewModel vm)
                {
                    vm.Logs.CollectionChanged -= ChangedLogs;
                    vm.Dispose();
                }
                if (_logTreeScrollViewer != null)
                    _logTreeScrollViewer.ScrollChanged -= LogScroll;
            };
        }

        private void LogScroll(object s, ScrollChangedEventArgs e)
        {
            if (s is not ScrollViewer sv) return;
            _autoScroll = e.ExtentHeightChange == 0 ? sv.VerticalOffset >= sv.ScrollableHeight - 2 : _autoScroll;
            if (e.ExtentHeightChange != 0 && _autoScroll) sv.ScrollToEnd();
        }

        private void ChangedLogs(object? _, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                foreach (var item in e.NewItems)
                    if (item is Domain.LogEntry log)
                    {
                        if (_lastParentNode != null && _lastParentNode != log) _lastParentNode.IsVisible = false;
                        log.IsVisible = true;
                        _lastParentNode = log;
                    }
                if (_autoScroll) ScrollToBottom();
            }));
        }

        private void ScrollToBottom() => GetScrollViewer(LogTree)?.ScrollToEnd();

        private static ScrollViewer? GetScrollViewer(DependencyObject? d)
        {
            if (d == null) return null;
            if (d is ScrollViewer sv) return sv;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                if (GetScrollViewer(VisualTreeHelper.GetChild(d, i)) is ScrollViewer child)
                    return child;
            }
            return null;
        }

    }

    public class ChangePadding : IValueConverter
    {
        public double Subtract { get; set; } = 20;
        private double? _last;

        public object Convert(object v, Type _, object __, CultureInfo ___)
        {
            if (v is double d)
            {
                if (_last.HasValue && Math.Abs(_last.Value - d) < 0.5)
                    return Binding.DoNothing;
                _last = d;
                return Math.Max(0, d - Subtract);
            }
            return v!;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value?.ToString();
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (Guid.TryParse(s, out var guid))
                return new KeyEntryId(guid.ToString());
            return new KeyEntryId(s);
        }
    }
}