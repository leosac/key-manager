using Leosac.KeyManager.Library;
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
    /// Interaction logic for KeyGenerationDialog.xaml
    /// </summary>
    public partial class KeyGenerationDialog : UserControl
    {
        public KeyGenerationDialog()
        {
            CeremonyTypes = new ObservableCollection<KeyCeremonyType>(Enum.GetValues<KeyCeremonyType>());

            InitializeComponent();
        }

        public ObservableCollection<KeyCeremonyType> CeremonyTypes { get; set; }

        public KeyCeremonyType SelectedCeremonyType
        {
            get { return (KeyCeremonyType)GetValue(SelectedCeremonyTypeProperty); }
            set { SetValue(SelectedCeremonyTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedCeremonyTypeProperty = DependencyProperty.Register(nameof(SelectedCeremonyType), typeof(KeyCeremonyType), typeof(KeyGenerationDialog),
            new FrameworkPropertyMetadata(KeyCeremonyType.Shamir));

        public int Fragments
        {
            get { return (int)GetValue(FragmentsProperty); }
            set { SetValue(FragmentsProperty, value); }
        }

        public static readonly DependencyProperty FragmentsProperty = DependencyProperty.Register(nameof(Fragments), typeof(int), typeof(KeyGenerationDialog),
            new FrameworkPropertyMetadata(3));

        public int KeyLength
        {
            get { return (int)GetValue(KeyLengthProperty); }
            set { SetValue(KeyLengthProperty, value); }
        }

        public static readonly DependencyProperty KeyLengthProperty = DependencyProperty.Register(nameof(KeyLength), typeof(int), typeof(KeyGenerationDialog),
            new FrameworkPropertyMetadata(16));

        public string? KeyValue
        {
            get { return (string)GetValue(KeyValueProperty); }
            set { SetValue(KeyValueProperty, value); }
        }

        public static readonly DependencyProperty KeyValueProperty = DependencyProperty.Register(nameof(KeyValue), typeof(string), typeof(KeyGenerationDialog));

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPassword_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
