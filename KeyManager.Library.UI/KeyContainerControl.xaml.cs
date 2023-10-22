using Leosac.KeyManager.Library.KeyStore;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyContainerControl.xaml
    /// </summary>
    public partial class KeyContainerControl : UserControl
    {
        public KeyContainerControl()
        {
            InitializeComponent();
        }

        private void RecreateKeyControl()
        {
            UserControl? control = null;
            if (keyContent.DataContext is Key key)
            {
                if (key.Tags.Contains(KeyEntryClass.Asymmetric.ToString()))
                {
                    var c = new AsymmetricKeyControl
                    {
                        Key = key,
                        ShowKeyLink = ShowKeyLink
                    };
                    control = c;
                }
                else
                {
                    var c = new SymmetricKeyControl
                    {
                        Key = key,
                        ShowKCV = ShowKCV,
                        ShowKeyLink = ShowKeyLink
                    };
                    control = c;
                }

                control.HorizontalAlignment = HorizontalAlignment.Stretch;
                control.VerticalAlignment = VerticalAlignment.Center;
            }
            keyContent.Content = control;
        }
        public bool ShowKCV
        {
            get { return (bool)GetValue(ShowKCVProperty); }
            set { SetValue(ShowKCVProperty, value); }
        }

        public static readonly DependencyProperty ShowKCVProperty = DependencyProperty.Register(nameof(ShowKCV), typeof(bool), typeof(KeyContainerControl),
            new FrameworkPropertyMetadata(true));

        public bool ShowKeyLink
        {
            get { return (bool)GetValue(ShowKeyLinkProperty); }
            set { SetValue(ShowKeyLinkProperty, value); }
        }

        public static readonly DependencyProperty ShowKeyLinkProperty = DependencyProperty.Register(nameof(ShowKeyLink), typeof(bool), typeof(KeyContainerControl),
            new FrameworkPropertyMetadata(true));

        public bool ShowKeyMaterials
        {
            get { return (bool)GetValue(ShowKeyMaterialsProperty); }
            set { SetValue(ShowKeyMaterialsProperty, value); }
        }

        public static readonly DependencyProperty ShowKeyMaterialsProperty = DependencyProperty.Register(nameof(ShowKeyMaterials), typeof(bool), typeof(KeyContainerControl),
            new FrameworkPropertyMetadata(true));

        private void KeyContent_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RecreateKeyControl();
        }

        private void KeyControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UserControl? control = null;
            if (DataContext is KeyVersion)
            {
                control = new KeyVersionExtControl
                {
                    DataContext = DataContext
                };
            }
            containerExtContent.Content = control;
        }
    }
}
