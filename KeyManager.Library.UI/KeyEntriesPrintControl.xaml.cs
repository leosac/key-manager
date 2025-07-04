using Leosac.KeyManager.Library.DivInput;
using Leosac.KeyManager.Library.KeyStore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyEntriesPrintControl.xaml
    /// </summary>
    public partial class KeyEntriesPrintControl : UserControl
    {
        public KeyEntriesPrintControl()
        {
            InitializeComponent();

            KeyEntries = new ObservableCollection<KeyEntry>();
        }

        public ObservableCollection<KeyEntry> KeyEntries
        {
            get { return (ObservableCollection<KeyEntry>)GetValue(KeyEntriesProperty); }
            set { SetValue(KeyEntriesProperty, value); }
        }

        public static readonly DependencyProperty KeyEntriesProperty = DependencyProperty.Register(nameof(KeyEntries), typeof(ObservableCollection<KeyEntry>), typeof(KeyEntriesPrintControl),
            new FrameworkPropertyMetadata());

        public string Note
        {
            get { return (string)GetValue(NoteProperty); }
            set { SetValue(NoteProperty, value); }
        }

        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(string), typeof(KeyEntriesPrintControl),
            new FrameworkPropertyMetadata(""));
    }
}
