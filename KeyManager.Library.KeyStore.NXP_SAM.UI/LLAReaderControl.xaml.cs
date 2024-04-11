using Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI
{
    /// <summary>
    /// Interaction logic for LLAReaderControl.xaml
    /// </summary>
    public partial class LLAReaderControl : UserControl
    {
        private readonly LibLogicalAccess.LibraryManager _lla;

        public LLAReaderControl()
        {
            _lla = LibLogicalAccess.LibraryManager.getInstance();
            ReaderProviders = new ObservableCollection<string>(_lla.getAvailableReaders().ToArray());
            ReaderUnits = new ObservableCollection<string>();

            InitializeComponent();
        }

        public string ReaderProvider
        {
            get { return (string)GetValue(ReaderProviderProperty); }
            set { SetValue(ReaderProviderProperty, value); }
        }
        public static readonly DependencyProperty ReaderProviderProperty = DependencyProperty.Register(nameof(ReaderProvider), typeof(string), typeof(LLAReaderControl),
            new FrameworkPropertyMetadata("PCSC"));

        public string ReaderUnit
        {
            get { return (string)GetValue(ReaderUnitProperty); }
            set { SetValue(ReaderUnitProperty, value); }
        }
        public static readonly DependencyProperty ReaderUnitProperty = DependencyProperty.Register(nameof(ReaderUnit), typeof(string), typeof(LLAReaderControl),
            new FrameworkPropertyMetadata(""));

        public ObservableCollection<string> ReaderProviders { get; set; }

        public ObservableCollection<string> ReaderUnits { get; set; }

        public void RefreshReaderList()
        {
            var prevru = ReaderUnit;
            ReaderUnits.Clear();
            var rp = _lla.getReaderProvider(ReaderProvider);
            var ruList = rp.getReaderList();
            foreach (var ru in ruList)
            {
                ReaderUnits.Add(ru.getName());
            }
            if (ReaderUnits.Contains(prevru))
            {
                ReaderUnit = prevru;
            }
        }

        private void cbReaderProvider_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshReaderList();
        }

        private void btnRefreshReaderUnits_Click(object sender, RoutedEventArgs e)
        {
            RefreshReaderList();
        }
    }
}
