using Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI
{
    /// <summary>
    /// Interaction logic for LLAReaderControl.xaml
    /// </summary>
    public partial class LLAReaderControl : UserControl
    {
        public LLAReaderControl()
        {
            InitializeComponent();

            DataContext = new LLAReaderViewModel();
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

        private void cbReaderProvider_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is LLAReaderViewModel model)
            {
                model.RefreshReaderList(this);
            }
        }

        private void btnRefreshReaderUnits_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LLAReaderViewModel model)
            {
                model.RefreshReaderList(this);
            }
        }
    }
}
