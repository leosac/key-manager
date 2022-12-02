using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using SecretSharingDotNet.Cryptography;
using SecretSharingDotNet.Math;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for AsymmetricKeyGenerationDialog.xaml
    /// </summary>
    public partial class AsymmetricKeyGenerationDialog : UserControl
    {
        public AsymmetricKeyGenerationDialog()
        {
            InitializeComponent();
        }

        public int KeySize
        {
            get { return (int)GetValue(KeySizeProperty); }
            set { SetValue(KeySizeProperty, value); }
        }

        public static readonly DependencyProperty KeySizeProperty = DependencyProperty.Register(nameof(KeySize), typeof(int), typeof(AsymmetricKeyGenerationDialog),
            new FrameworkPropertyMetadata(16));

        public string? KeyValue
        {
            get { return (string)GetValue(KeyValueProperty); }
            set { SetValue(KeyValueProperty, value); }
        }

        public static readonly DependencyProperty KeyValueProperty = DependencyProperty.Register(nameof(KeyValue), typeof(string), typeof(AsymmetricKeyGenerationDialog));

        public Key SeedKey
        {
            get { return (Key)GetValue(SeedKeyProperty); }
            set { SetValue(SeedKeyProperty, value); }
        }

        public static readonly DependencyProperty SeedKeyProperty = DependencyProperty.Register(nameof(KeyValue), typeof(Key), typeof(AsymmetricKeyGenerationDialog),
            new PropertyMetadata(new Key()));

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnCreateFromSeed_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
