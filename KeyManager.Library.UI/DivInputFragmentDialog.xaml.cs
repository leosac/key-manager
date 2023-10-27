using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for DivInputFragmentDialog.xaml
    /// </summary>
    public partial class DivInputFragmentDialog : UserControl
    {
        public DivInputFragmentDialog()
        {
            InitializeComponent();
        }

        public Control? DivInputFragmentControl
        {
            get { return (Control)GetValue(DivInputFragmentControlProperty); }
            set { SetValue(DivInputFragmentControlProperty, value); }
        }

        public static readonly DependencyProperty DivInputFragmentControlProperty = DependencyProperty.Register(nameof(DivInputFragmentControl), typeof(Control), typeof(DivInputFragmentDialog));

    }
}
