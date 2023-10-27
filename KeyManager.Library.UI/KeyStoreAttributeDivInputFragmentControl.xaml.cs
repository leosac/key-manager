using Leosac.KeyManager.Library.DivInput;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyStoreAttributeDivInputFragmentControl.xaml
    /// </summary>
    public partial class KeyStoreAttributeDivInputFragmentControl : UserControl
    {
        public KeyStoreAttributeDivInputFragmentControl()
        {
            InitializeComponent();
        }

        public KeyStoreAttributeDivInputFragment Fragment
        {
            get { return (KeyStoreAttributeDivInputFragment)GetValue(FragmentProperty); }
            set { SetValue(FragmentProperty, value); }
        }

        public static readonly DependencyProperty FragmentProperty = DependencyProperty.Register(nameof(Fragment), typeof(KeyStoreAttributeDivInputFragment), typeof(KeyStoreAttributeDivInputFragmentControl),
            new FrameworkPropertyMetadata());
    }
}
