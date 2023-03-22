using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                    var c = new AsymmetricKeyControl();
                    c.Key = key;
                    c.ShowKeyLink = ShowKeyLink;
                    control = c;
                }
                else
                {
                    var c = new SymmetricKeyControl();
                    c.Key = key;
                    c.ShowKCV = ShowKCV;
                    c.ShowKeyLink = ShowKeyLink;
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

        private void keyContent_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RecreateKeyControl();
        }

        private void keyControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UserControl? control = null;
            if (DataContext is KeyVersion)
            {
                control = new KeyVersionExtControl();
            }

            if (control != null)
                control.DataContext = DataContext;

            containerExtContent.Content = control;
        }
    }
}
