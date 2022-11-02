using Leosac.KeyManager.Library.UI.Domain;
using Leosac.KeyManager.Library.Policy;
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
    /// Interaction logic for KeyControl.xaml
    /// </summary>
    public partial class KeyControl : UserControl
    {
        public KeyControl()
        {
            InitializeComponent();
        }

        public bool ShowPassword
        {
            get { return (bool)GetValue(ShowPasswordProperty); }
            set { SetValue(ShowPasswordProperty, value); }
        }

        public static readonly DependencyProperty ShowPasswordProperty = DependencyProperty.Register("ShowPassword", typeof(bool), typeof(KeyControl),
            new FrameworkPropertyMetadata(false));

        public KeyManager.Library.Key Key
        {
            get { return (KeyManager.Library.Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(KeyManager.Library.Key), typeof(KeyControl),
            new FrameworkPropertyMetadata(new KeyManager.Library.Key()));

        private void CopyBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(KeyValue.Password);
        }

        private void KeyValue_PasswordChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                Key?.ValidatePolicies();
                tbxKeyError.Text = String.Empty;
            }
            catch (KeyPolicyException ex)
            {
                tbxKeyError.Text = ex.Message;
            }
        }
    }
}
