using Leosac.KeyManager.Library.DivInput;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for StaticDivInputFragmentControl.xaml
    /// </summary>
    public partial class StaticDivInputFragmentControl : UserControl
    {
        public StaticDivInputFragmentControl()
        {
            InitializeComponent();
        }

        public StaticDivInputFragment Fragment
        {
            get { return (StaticDivInputFragment)GetValue(FragmentProperty); }
            set { SetValue(FragmentProperty, value); }
        }

        public static readonly DependencyProperty FragmentProperty = DependencyProperty.Register(nameof(Fragment), typeof(StaticDivInputFragment), typeof(StaticDivInputFragmentControl),
            new FrameworkPropertyMetadata());
    }
}
