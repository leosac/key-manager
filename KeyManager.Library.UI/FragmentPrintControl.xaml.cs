using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for FragmentPrintControl.xaml
    /// </summary>
    public partial class FragmentPrintControl : UserControl
    {
        public FragmentPrintControl()
        {
            InitializeComponent();
        }

        public string Fragment
        {
            get { return (string)GetValue(FragmentProperty); }
            set { SetValue(FragmentProperty, value); }
        }

        public static readonly DependencyProperty FragmentProperty = DependencyProperty.Register(nameof(Fragment), typeof(string), typeof(FragmentPrintControl));

        public string Note
        {
            get { return (string)GetValue(NoteProperty); }
            set { SetValue(NoteProperty, value); }
        }

        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(string), typeof(FragmentPrintControl),
            new FrameworkPropertyMetadata(""));
    }
}
