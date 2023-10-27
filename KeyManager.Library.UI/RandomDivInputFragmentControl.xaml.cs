using Leosac.KeyManager.Library.DivInput;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for RandomDivInputFragmentControl.xaml
    /// </summary>
    public partial class RandomDivInputFragmentControl : UserControl
    {
        public RandomDivInputFragmentControl()
        {
            InitializeComponent();
        }

        public RandomDivInputFragment Fragment
        {
            get { return (RandomDivInputFragment)GetValue(FragmentProperty); }
            set { SetValue(FragmentProperty, value); }
        }

        public static readonly DependencyProperty FragmentProperty = DependencyProperty.Register(nameof(Fragment), typeof(RandomDivInputFragment), typeof(RandomDivInputFragmentControl),
            new FrameworkPropertyMetadata());
    }
}
