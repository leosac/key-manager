using Leosac.KeyManager.Library.Plugin.UI.Domain;

namespace Leosac.KeyManager.Domain
{
    public class PlanRegistrationWindowViewModel : ViewModelBase
    {
        public PlanRegistrationWindowViewModel()
        {
            var plan = MaintenancePlan.GetSingletonInstance();
            _key = plan.LicenseKey;
            _uuid = plan.GetUUID();
            _offlineRegistrationUrl = MaintenancePlan.GetOfflineRegistrationUrl(_key, _uuid);
        }

        private string? _key;
        private string? _email;
        private string? _uuid;
        private string? _code;
        private string _offlineRegistrationUrl;
        private string? _lastError;

        public string? Key
        {
            get => _key;
            set
            {
                SetProperty(ref _key, value);
                OfflineRegistrationUrl = MaintenancePlan.GetOfflineRegistrationUrl(_key, UUID);
            }
        }

        public string? Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string? UUID
        {
            get => _uuid;
            private set => SetProperty(ref _uuid, value);
        }

        public string? Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        public string OfflineRegistrationUrl
        {
            get => _offlineRegistrationUrl;
            private set => SetProperty(ref _offlineRegistrationUrl, value);
        }

        public string? LastError
        {
            get => _lastError;
            set => SetProperty(ref _lastError, value);
        }
    }
}
