using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public partial class PublishBatchOptions : ObservableObject
    {
        public PublishBatchOptions()
        {
            _count = 1;
            _maxCount = 9999;
            _timeoutWaitMin = 5;
            _timeoutWaitMax = 600;
            _timeoutWaitSeconds = 5;
            _continuousMode = true;
            _confirmEachUnit = true;
            _isEnabled = false;
            _loggingVerbosity = true;
            _retryOnFailure = false;
            _timeoutWait = false;
            _notifications = true;
            UpdateFlowStatus();
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (SetProperty(ref _isEnabled, value))
                    UpdateFlowStatus();
            }
        }

        private bool _isSupported;
        public bool IsSupported
        {
            get => _isSupported;
            set
            {
                if (SetProperty(ref _isSupported, value))
                    UpdateFlowStatus();
            }
        }

        private int _count;
        public int Count
        {
            get => _count;
            set
            {
                SetProperty(ref _count, Math.Clamp(value, 1, MaxCount));
                OnPropertyChanged(nameof(Summary));
            }
        }

        private int _maxCount;
        public int MaxCount
        {
            get => _maxCount;
            set => SetProperty(ref _maxCount, value);
        }

        private int _timeoutWaitSeconds;
        public int TimeoutWaitSeconds
        {
            get => _timeoutWaitSeconds;
            set
            {
                if (value < TimeoutWaitMin) value = TimeoutWaitMin;
                if (value > TimeoutWaitMax) value = TimeoutWaitMax;

                SetProperty(ref _timeoutWaitSeconds, value);
                OnPropertyChanged(nameof(Summary));
            }
        }

        private int _timeoutWaitMin;
        public int TimeoutWaitMin
        {
            get => _timeoutWaitMin;
            set => SetProperty(ref _timeoutWaitMin, value);
        }

        private int _timeoutWaitMax;
        public int TimeoutWaitMax
        {
            get => _timeoutWaitMax;
            set => SetProperty(ref _timeoutWaitMax, value);
        }

        private bool _continuousMode;
        public bool ContinuousMode
        {
            get => _continuousMode;
            set
            {
                SetProperty(ref _continuousMode, value);
                OnPropertyChanged(nameof(Summary));
            }
        }

        private bool _confirmEachUnit;
        public bool ConfirmEachUnit
        {
            get => _confirmEachUnit;
            set => SetProperty(ref _confirmEachUnit, value);
        }

        private bool _loggingVerbosity;
        public bool LoggingVerbosity
        {
            get => _loggingVerbosity;
            set => SetProperty(ref _loggingVerbosity, value);
        }

        private bool _retryOnFailure;
        public bool RetryOnFailure
        {
            get => _retryOnFailure;
            set => SetProperty(ref _retryOnFailure, value);
        }

        private bool _timeoutWait;
        public bool TimeoutWait
        {
            get => _timeoutWait;
            set
            {
                SetProperty(ref _timeoutWait, value);
                OnPropertyChanged(nameof(Summary));
            }
        }

        private bool _notifications;
        public bool Notifications
        {
            get => _notifications;
            set => SetProperty(ref _notifications, value);
        }


        private string _flowStatusMessage = string.Empty;
        public string FlowStatusMessage
        {
            get => _flowStatusMessage;
            private set => SetProperty(ref _flowStatusMessage, value);
        }

        public void UpdateFlowStatus() => FlowStatusMessage = !IsSupported ? "Not supported" : !IsEnabled ? "Disabled" : "Enabled";

        public string Summary =>
            $"{Count} unit(s) • {(ContinuousMode ? "Continuous mode" : "Manual mode")}" +
            $"{(TimeoutWait ? $" • timeout {TimeoutWaitSeconds}s" : "")}";
    }
}
