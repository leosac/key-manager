using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for FolderBrowserDialog.xaml
    /// </summary>
    public partial class FolderBrowserDialog : Window
    {
        public FolderBrowserDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var model = DataContext as FolderBrowserDialogViewModel;
            if (model != null)
            {
                if (model.SelectedDrive == null)
                {
                    if (model.Drives.Count > 0)
                    {
                        model.SelectedDrive = model.Drives[0];
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
