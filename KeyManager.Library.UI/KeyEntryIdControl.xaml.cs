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
    /// Interaction logic for KeyEntryIdControl.xaml
    /// </summary>
    public partial class KeyEntryIdControl : UserControl
    {
        public KeyEntryIdControl()
        {
            InitializeComponent();
        }

        public KeyManager.Library.KeyStore.KeyEntryId KeyEntryId
        {
            get { return (KeyManager.Library.KeyStore.KeyEntryId)GetValue(KeyEntryIdProperty); }
            set { SetValue(KeyEntryIdProperty, value); }
        }

        public static readonly DependencyProperty KeyEntryIdProperty = DependencyProperty.Register(nameof(KeyEntryId), typeof(KeyManager.Library.KeyStore.KeyEntryId), typeof(KeyEntryIdControl),
            new FrameworkPropertyMetadata(new KeyManager.Library.KeyStore.KeyEntryId()));
    }
}
