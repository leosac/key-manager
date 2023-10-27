using Leosac.KeyManager.Library.DivInput;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for PaddingDivInputFragmentControl.xaml
    /// </summary>
    public partial class PaddingDivInputFragmentControl : UserControl
    {
        public PaddingDivInputFragmentControl()
        {
            InitializeComponent();
        }

        public PaddingDivInputFragment Fragment
        {
            get { return (PaddingDivInputFragment)GetValue(FragmentProperty); }
            set { SetValue(FragmentProperty, value); }
        }

        public static readonly DependencyProperty FragmentProperty = DependencyProperty.Register(nameof(Fragment), typeof(PaddingDivInputFragment), typeof(PaddingDivInputFragmentControl),
            new FrameworkPropertyMetadata());
    }
}
