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
    /// Interaction logic for KeyVersionControl.xaml
    /// </summary>
    public partial class KeyVersionControl : UserControl
    {
        public KeyVersionControl()
        {
            InitializeComponent();
        }

        public KeyManager.Library.KeyStore.KeyVersion KeyVersion
        {
            get { return (KeyManager.Library.KeyStore.KeyVersion)GetValue(KeyVersionProperty); }
            set { SetValue(KeyVersionProperty, value); }
        }

        public static readonly DependencyProperty KeyVersionProperty = DependencyProperty.Register("KeyVersion", typeof(KeyManager.Library.KeyStore.KeyVersion), typeof(KeyVersionControl),
            new FrameworkPropertyMetadata(new KeyManager.Library.KeyStore.KeyVersion()));
    }
}
