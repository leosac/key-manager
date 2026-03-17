using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class LogEntry : INotifyPropertyChanged
    {
        public LogEntry(string message, LogLevel level = LogLevel.Info)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Level = level;
            IsVisible = true;
        }

        public DateTime Time { get; } = DateTime.Now;

        private string _message = string.Empty;
        public string Message
        {
            get => _message;
            set => SetAndNotify(ref _message, value, nameof(Message), nameof(DisplayText));
        }

        private LogLevel _level;
        public LogLevel Level
        {
            get => _level;
            set => SetAndNotify(ref _level, value, nameof(Level), nameof(Icon), nameof(DisplayText));
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetAndNotify(ref _isExpanded, value, nameof(IsExpanded));
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set => SetAndNotify(ref _isVisible, value, nameof(IsVisible));
        }

        public string Icon => Level switch
        {
            LogLevel.Info => "ℹ",
            LogLevel.Warning => "⚠",
            LogLevel.Error => "❌",
            LogLevel.Success => "✔",
            _ => ""
        };

        public ObservableCollection<LogEntry> Nodes { get; } = new ObservableCollection<LogEntry>();

        public string DisplayText => $"{Time:HH:mm:ss} {Icon} {Level.ToString().ToUpper(),-7} {Message}";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void SetAndNotify<T>(ref T field, T value, params string[] propertyNames)
        {
            if (Equals(field, value)) return;
            field = value;
            foreach (var p in propertyNames) OnPropertyChanged(p);
        }
    }
}