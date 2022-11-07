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
    /// Interaction logic for KeyPrintControl.xaml
    /// </summary>
    public partial class KeyPrintControl : UserControl
    {
        public KeyPrintControl()
        {
            InitializeComponent();
        }

        public Key Key
        {
            get { return (Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(Key), typeof(Key), typeof(KeyPrintControl));

        public string KeyChecksum
        {
            get { return (string)GetValue(KeyChecksumProperty); }
            set { SetValue(KeyChecksumProperty, value); }
        }

        public static readonly DependencyProperty KeyChecksumProperty = DependencyProperty.Register(nameof(KeyChecksum), typeof(string), typeof(KeyPrintControl),
            new FrameworkPropertyMetadata(""));

        public string Note
        {
            get { return (string)GetValue(NoteProperty); }
            set { SetValue(NoteProperty, value); }
        }

        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(string), typeof(KeyPrintControl),
            new FrameworkPropertyMetadata(""));
    }
}
