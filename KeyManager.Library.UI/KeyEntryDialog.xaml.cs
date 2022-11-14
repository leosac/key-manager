using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for KeyEntryControl.xaml
    /// </summary>
    public partial class KeyEntryDialog : UserControl
    {
        public KeyEntryDialog()
        {
            InitializeComponent();
        }

        private void KeyEntryType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is KeyEntryDialogViewModel model && model.KeyEntry != null)
            {
                model.Variants.Clear();
                var variants = model.KeyEntry.GetAllVariants();
                foreach(var variant in variants)
                {
                    model.Variants.Add(variant);
                }
            }
        }
    }
}
