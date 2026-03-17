using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for PublishBatchOptionsControl.xaml
    /// </summary>
    public partial class PublishBatchOptionsControl : UserControl
    {
        public PublishBatchOptionsControl()
        {
            InitializeComponent();
        }

        public PublishBatchOptions BatchOptions
        {
            get => (PublishBatchOptions)GetValue(BatchOptionsProperty);
            set => SetValue(BatchOptionsProperty, value);
        }

        public static readonly DependencyProperty BatchOptionsProperty =
            DependencyProperty.Register(
                nameof(BatchOptions),
                typeof(PublishBatchOptions),
                typeof(PublishBatchOptionsControl),
                new PropertyMetadata(null));



    }
}
