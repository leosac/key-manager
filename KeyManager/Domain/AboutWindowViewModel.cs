using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Domain
{
    public class AboutWindowViewModel : ViewModelBase
    {
        public class Library
        {
            public Library(string name, string license, string? description = null, string? url = null)
            {
                Name = name;
                License = license;
                Description = description;
                Url = url;
            }

            public string Name { get; private set; }

            public string License { get; private set; }

            public string? Description { get; private set; }


            public string? Url { get; private set; }
        }

        public AboutWindowViewModel()
        {
            GetInfoFromAssembly();

            _settings = KMSettings.GetSingletonInstance();
            _autoUpdate = new AutoUpdate();

            Libraries = new ObservableCollection<Library>(new[]
            {
                new Library("Microsoft.NETCore.App", "MIT", ".NET Core SDK", "https://github.com/dotnet/runtime"),
                new Library("Microsoft.WindowsDesktop.App.WPF", "MIT", ".NET Core UI Framework", "https://github.com/dotnet/wpf"),
                new Library("log4net", "Apache v2", "Logging library", "https://logging.apache.org/log4net/"),
                new Library("MaterialDesignInXaml", "MIT", "Graphic library", "http://materialdesigninxaml.net/"),
                new Library("Json.NET", "MIT", "JSON library", "https://www.newtonsoft.com/json"),
                new Library("SecretSharingDotNet", "MIT", "Shamir Secret Sharing library", "https://github.com/shinji-san/SecretSharingDotNet"),
                new Library("Net.Codecrete.QrCodeGenerator", "MIT", "QR Code Generator library", "https://github.com/manuelbl/QrCodeGenerator"),
                new Library("SkiaSharp", "MIT", "Graphic library", "https://github.com/mono/SkiaSharp"),
                new Library("Pkcs11Interop", "Apache v2", "PKCS#11 libraries wrapper", "https://github.com/Pkcs11Interop/Pkcs11Interop"),
                new Library("LibLogicalAccess", "LGPL", "RFID/NFC Library", "https://github.com/islog/liblogicalaccess"),
                new Library("zlib", "zlib", "compression library", "https://zlib.net/"),
                new Library("openssl", "OpenSSL and SSLeay", "cryptographic library", "https://www.openssl.org/"),
                new Library("boost", "Boost Software", "cross-platform C++ library", "https://www.boost.org/"),
                new Library("nlohmann/json", "MIT", "JSON library", "https://github.com/nlohmann/json"),
                new Library("Crc32.NET", "MIT", "CRC32 library", "https://github.com/force-net/Crc32.NET")
            });

            var plan = MaintenancePlan.GetSingletonInstance();
            _isActivePlan = plan.IsActivePlan();
            _expirationDate = plan.ExpirationDate;
        }

        private string? _softwareName;
        private string? _softwareVersion;
        private KMSettings _settings;
        private AutoUpdate _autoUpdate;
        private bool _isActivePlan;
        private DateTime? _expirationDate;

        public string? SoftwareName
        {
            get => _softwareName;
            set => SetProperty(ref _softwareName, value);
        }

        public string? SoftwareVersion
        {
            get => _softwareVersion;
            set => SetProperty(ref _softwareVersion, value);
        }

        public KMSettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        public AutoUpdate AutoUpdate
        {
            get => _autoUpdate;
            set => SetProperty(ref _autoUpdate, value);
        }

        public bool IsActivePlan
        {
            get => _isActivePlan;
            set => SetProperty(ref _isActivePlan, value);
        }

        public DateTime? ExpirationDate
        {
            get => _expirationDate;
            set => SetProperty(ref _expirationDate, value);
        }

        public ObservableCollection<Library> Libraries { get; }

        private void GetInfoFromAssembly()
        {
            var fvi = KMSettings.GetFileVersionInfo();
            _softwareName = fvi?.ProductName;
            _softwareVersion = fvi?.ProductVersion;
        }
    }
}
