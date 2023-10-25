using Leosac.KeyManager.Library.UI.Domain;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for KeyCeremonySetupControl.xaml
    /// </summary>
    public partial class KeyCeremonySetupControl : UserControl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public KeyCeremonySetupControl()
        {
            SecretSharings = new ObservableCollection<SecretSharing.SecretSharingBase>(SecretSharing.SecretSharingBase.GetAll());
            SelectedSecretSharing = SecretSharings.FirstOrDefault();

            InitializeComponent();
        }

        public ObservableCollection<SecretSharing.SecretSharingBase> SecretSharings { get; set; }

        public SecretSharing.SecretSharingBase? SelectedSecretSharing
        {
            get { return (SecretSharing.SecretSharingBase)GetValue(SelectedSecretSharingProperty); }
            set { SetValue(SelectedSecretSharingProperty, value); }
        }

        public static readonly DependencyProperty SelectedSecretSharingProperty = DependencyProperty.Register(nameof(SelectedSecretSharing), typeof(SecretSharing.SecretSharingBase), typeof(KeyCeremonySetupControl));

        public int Fragments
        {
            get { return (int)GetValue(FragmentsProperty); }
            set { SetValue(FragmentsProperty, value); }
        }

        public static readonly DependencyProperty FragmentsProperty = DependencyProperty.Register(nameof(Fragments), typeof(int), typeof(KeyCeremonySetupControl),
            new FrameworkPropertyMetadata(3));

        public string? KeyValue
        {
            get { return (string)GetValue(KeyValueProperty); }
            set { SetValue(KeyValueProperty, value); }
        }

        public static readonly DependencyProperty KeyValueProperty = DependencyProperty.Register(nameof(KeyValue), typeof(string), typeof(KeyCeremonySetupControl));

        private void BtnUnionCeremony_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSecretSharing != null)
            {
                var model = new KeyCeremonyDialogViewModel
                {
                    IsReunification = true,
                    Fragments = new ObservableCollection<string>(new string[Fragments])
                };
                var dialog = new KeyCeremonyDialog
                {
                    DataContext = model
                };
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        var key = SelectedSecretSharing.ComputeFragments(model.Fragments.Select(f => f.Replace(Environment.NewLine, string.Empty)).ToArray());
                        KeyValue = key != null ? Convert.ToHexString(key) : string.Empty;
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error during the key ceremony.", ex);
                        MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnSharingCeremony_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSecretSharing != null && !string.IsNullOrEmpty(KeyValue))
            {
                var model = new KeyCeremonyDialogViewModel
                {
                    IsReunification = false
                };
                var fragments = SelectedSecretSharing.CreateFragments(Convert.FromHexString(KeyValue), Fragments);
                if (fragments != null)
                {
                    foreach (var fragment in fragments)
                    {
                        model.Fragments.Add(fragment);
                    }

                    var dialog = new KeyCeremonyDialog
                    {
                        DataContext = model
                    };
                    dialog.ShowDialog();
                }
            }
        }
    }
}
