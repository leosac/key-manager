using Leosac.KeyManager.Library.KeyStore;
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
    /// Interaction logic for OpenFavoriteControl.xaml
    /// </summary>
    public partial class OpenFavoriteControl : UserControl
    {
        public OpenFavoriteControl()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(OpenFavoriteControl),
            new FrameworkPropertyMetadata(""));

        public ICommand? Command
        {
            get { return (ICommand?)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(OpenFavoriteControl));

        public KeyStoreProperties? Properties
        {
            get { return (KeyStoreProperties?)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register(nameof(Properties), typeof(KeyStoreProperties), typeof(OpenFavoriteControl));
    }
}
