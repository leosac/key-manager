using Leosac.KeyManager.Library.UI.Domain;
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
    /// Interaction logic for KeyCeremonyDialog.xaml
    /// </summary>
    public partial class KeyCeremonyDialog : Window
    {
        public KeyCeremonyDialog()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            while (transition.Items.Count > 2)
            {
                transition.Items.RemoveAt(1);
            }

            if (DataContext is KeyCeremonyDialogViewModel model)
            {
                for (int i = 0; i < model.Fragments.Count; ++i)
                {
                    transition.Items.Insert(i + 1, new KeyCeremonyFragmentControl() { Fragment = i + 1 });
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is KeyCeremonyDialogViewModel model)
            {
                for (int i = 0; i < model.Fragments.Count && (i + 1) < transition.Items.Count; ++i)
                {
                    if (transition.Items[i + 1] is KeyCeremonyFragmentControl fragmentControl)
                    {
                        model.Fragments[i] = fragmentControl.FragmentValue;
                    }
                }
                this.DialogResult = true;
            }
        }
    }
}
