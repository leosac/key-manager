using Leosac.KeyManager.Library.UI.Domain;
using Leosac.KeyManager.Library.Policy;
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
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using Leosac.KeyManager.Library.KeyStore;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for AsymmetricKeyControl.xaml
    /// </summary>
    public partial class AsymmetricKeyControl : UserControl
    {
        public AsymmetricKeyControl()
        {
            InitializeComponent();
        }

        public KeyManager.Library.Key Key
        {
            get { return (KeyManager.Library.Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(Key), typeof(KeyManager.Library.Key), typeof(AsymmetricKeyControl),
            new FrameworkPropertyMetadata(new KeyManager.Library.Key()));

        public bool ShowKeyLink
        {
            get { return (bool)GetValue(ShowKeyLinkProperty); }
            set { SetValue(ShowKeyLinkProperty, value); }
        }

        public static readonly DependencyProperty ShowKeyLinkProperty = DependencyProperty.Register(nameof(ShowKeyLink), typeof(bool), typeof(AsymmetricKeyControl),
            new FrameworkPropertyMetadata(true));
    }
}
