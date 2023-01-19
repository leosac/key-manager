using Leosac.KeyManager.Library.Wizard.SAMAccessControl.Domain;
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
using System.Windows.Shapes;

namespace Leosac.KeyManager.Library.Wizard.SAMAccessControl
{
    /// <summary>
    /// Interaction logic for SAMAccessControlWizardWindow.xaml
    /// </summary>
    public partial class SAMAccessControlWizardWindow : Window
    {
        public SAMAccessControlWizardWindow()
        {
            InitializeComponent();

            DataContext = new SAMAccessControlWizardWindowViewModel();
        }
    }
}
