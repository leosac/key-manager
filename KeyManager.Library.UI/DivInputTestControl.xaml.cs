using Leosac.KeyManager.Library.DivInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for DivInputTestControl.xaml
    /// </summary>
    public partial class DivInputTestControl : UserControl
    {
        public DivInputTestControl()
        {
            InitializeComponent();
        }

        public ObservableCollection<DivInputFragment> DivInput
        {
            get { return (ObservableCollection<DivInputFragment>)GetValue(DivInputProperty); }
            set { SetValue(DivInputProperty, value); }
        }

        public static readonly DependencyProperty DivInputProperty = DependencyProperty.Register(nameof(DivInput), typeof(ObservableCollection<DivInputFragment>), typeof(DivInputTestControl),
            new FrameworkPropertyMetadata(new ObservableCollection<DivInputFragment>()));

        public string DivInputResult
        {
            get { return (string)GetValue(DivInputResultProperty); }
            set { SetValue(DivInputResultProperty, value); }
        }

        public static readonly DependencyProperty DivInputResultProperty = DependencyProperty.Register(nameof(DivInputResult), typeof(string), typeof(DivInputTestControl),
            new FrameworkPropertyMetadata(""));
    }
}
