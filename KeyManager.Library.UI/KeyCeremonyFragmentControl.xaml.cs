using Microsoft.Win32;
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
    /// Interaction logic for KeyCeremonyFragmentControl.xaml
    /// </summary>
    public partial class KeyCeremonyFragmentControl : UserControl
    {
        public KeyCeremonyFragmentControl()
        {
            InitializeComponent();
        }

        public int Fragment
        {
            get { return (int)GetValue(FragmentProperty); }
            set { SetValue(FragmentProperty, value); }
        }

        public static readonly DependencyProperty FragmentProperty = DependencyProperty.Register(nameof(Fragment), typeof(int), typeof(KeyCeremonyFragmentControl),
            new FrameworkPropertyMetadata(1));

        public string FragmentValue
        {
            get { return (string)GetValue(FragmentValueProperty); }
            set { SetValue(FragmentValueProperty, value); }
        }

        public static readonly DependencyProperty FragmentValueProperty = DependencyProperty.Register(nameof(FragmentValue), typeof(string), typeof(KeyCeremonyFragmentControl),
            new FrameworkPropertyMetadata(""));

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(nameof(FilePath), typeof(string), typeof(KeyCeremonyFragmentControl),
            new FrameworkPropertyMetadata(""));

        private void btnBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                FilePath = ofd.FileName;
            }
        }
    }
}
