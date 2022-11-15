using Leosac.KeyManager.Library.UI;
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

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI
{
    /// <summary>
    /// Interaction logic for ChangeKeyReferenceControl.xaml
    /// </summary>
    public partial class ChangeKeyReferenceControl : UserControl
    {
        public ChangeKeyReferenceControl()
        {
            InitializeComponent();
        }

        public byte ChangeKeyRefId
        {
            get { return (byte)GetValue(ChangeKeyRefIdProperty); }
            set { SetValue(ChangeKeyRefIdProperty, value); }
        }

        public static readonly DependencyProperty ChangeKeyRefIdProperty = DependencyProperty.Register(nameof(ChangeKeyRefId), typeof(byte), typeof(ChangeKeyReferenceControl),
            new FrameworkPropertyMetadata((byte)0));

        public byte ChangeKeyRefVersion
        {
            get { return (byte)GetValue(ChangeKeyRefVersionProperty); }
            set { SetValue(ChangeKeyRefVersionProperty, value); }
        }

        public static readonly DependencyProperty ChangeKeyRefVersionProperty = DependencyProperty.Register(nameof(ChangeKeyRefVersion), typeof(byte), typeof(ChangeKeyReferenceControl),
            new FrameworkPropertyMetadata((byte)0));
    }
}
