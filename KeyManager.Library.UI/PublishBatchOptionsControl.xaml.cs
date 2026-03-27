using Leosac.KeyManager.Library.UI.Domain;
using System.Windows;
using System.Windows.Controls;

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
