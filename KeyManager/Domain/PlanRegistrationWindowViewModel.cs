using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
        private string? _uuid;
        private string? _code;
        private string _offlineRegistrationUrl;

        public string? Key
        {
            get => _key;
            set
            {
                SetProperty(ref _key, value);
                OfflineRegistrationUrl = MaintenancePlan.GetOfflineRegistrationUrl(_key, UUID);
            }
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
    }
}
