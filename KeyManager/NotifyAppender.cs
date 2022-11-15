using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager
{
    public class NotifyAppender : AppenderSkeleton, INotifyPropertyChanged
    {
        public NotifyAppender()
        {
            
        }

        #region Members and events
        private static object objlock = new object();
        private static string _notification = string.Empty;
        private static int _maxlines = 100;
        private event PropertyChangedEventHandler? _propertyChanged;

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        #endregion

        /// <summary>
        /// Get the joined notification message.
        /// </summary>
        public string Notification
        {
            get => String.Join(Environment.NewLine, NotificationLines);
        }

        /// <summary>
        /// Get or set the notification message lines.
        /// </summary>
        public ObservableCollection<string> NotificationLines = new ObservableCollection<string>();

        /// <summary>
        /// Get or set the maximum number of lines.
        /// </summary>
        public int MaxLines
        {
            get => _maxlines;
            set
            {
                if (_maxlines != value)
                {
                    _maxlines = value;
                    OnChange();
                }
            }
        }

        /// <summary>
        /// Raise the change notification.
        /// </summary>
        private void OnChange(string? propertyName = null)
        {
            var handler = _propertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Get a reference to the log instance.
        /// </summary>
        public static NotifyAppender? Appender
        {
            get
            {
                foreach (ILog log in LogManager.GetCurrentLoggers())
                {
                    foreach (IAppender appender in log.Logger.Repository.GetAppenders())
                    {
                        if (appender is NotifyAppender notifyAppender)
                        {
                            return notifyAppender;
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Append the log information to the notification.
        /// </summary>
        /// <param name="loggingEvent">The log event.</param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            Layout.Format(writer, loggingEvent);
            lock (objlock)
            {
                NotificationLines.Add(writer.ToString());
                var count = NotificationLines.Count - MaxLines;
                if (loggingEvent.ExceptionObject != null)
                {
                    NotificationLines.Add(loggingEvent.ExceptionObject.ToString());
                }
                if (count > 0)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        NotificationLines.RemoveAt(0);
                    }
                }
                OnChange(nameof(Notification));
            }
        }

        /// <summary>
        /// Clear all the notifications
        /// </summary>
        public void ClearNotifications()
        {
            lock (objlock)
            {
                NotificationLines.Clear();
                OnChange(nameof(Notification));
            }
        }
    }
}
