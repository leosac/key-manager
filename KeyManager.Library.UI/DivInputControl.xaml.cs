using Leosac.KeyManager.Library.DivInput;
using log4net;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for DivInputControl.xaml
    /// </summary>
    public partial class DivInputControl : UserControl
    {
        public DivInputControl()
        {
            InitializeComponent();

            DivInput = new ObservableCollection<DivInputFragment>();
            FragmentTypes = new ObservableCollection<DivInputFragment>(DivInputFragment.GetAll());
        }

        public ObservableCollection<DivInputFragment> DivInput
        {
            get { return (ObservableCollection<DivInputFragment>)GetValue(DivInputProperty); }
            set { SetValue(DivInputProperty, value); }
        }

        public static readonly DependencyProperty DivInputProperty = DependencyProperty.Register(nameof(DivInput), typeof(ObservableCollection<DivInputFragment>), typeof(DivInputControl),
            new FrameworkPropertyMetadata());

        public ObservableCollection<DivInputFragment> FragmentTypes
        {
            get { return (ObservableCollection<DivInputFragment>)GetValue(FragmentTypesProperty); }
            set { SetValue(FragmentTypesProperty, value); }
        }

        public static readonly DependencyProperty FragmentTypesProperty = DependencyProperty.Register(nameof(FragmentTypes), typeof(ObservableCollection<DivInputFragment>), typeof(DivInputControl),
            new FrameworkPropertyMetadata());

        public DivInputFragment? SelectedFragmentType
        {
            get { return (DivInputFragment?)GetValue(SelectedFragmentTypeProperty); }
            set { SetValue(SelectedFragmentTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedFragmentTypeProperty = DependencyProperty.Register(nameof(SelectedFragmentType), typeof(DivInputFragment), typeof(DivInputControl),
            new FrameworkPropertyMetadata());

        private void RemoveFragmentClick(object sender, RoutedEventArgs e)
        {
            if (e.Source is MaterialDesignThemes.Wpf.Chip chip)
            {
                var fragment = chip.Content as DivInputFragment;
                if (fragment != null)
                {
                    DivInput.Remove(fragment);
                }
            }
        }

        private async void EditFragmentClick(object sender, RoutedEventArgs e)
        {
            if (e.Source is MaterialDesignThemes.Wpf.Chip chip)
            {
                var fragment = chip.Content as DivInputFragment;
                if (fragment != null)
                {
                    UserControl? control = null;

                    if (fragment is KeyStoreAttributeDivInputFragment attributeFragment)
                    {
                        control = new KeyStoreAttributeDivInputFragmentControl()
                        {
                            Fragment = attributeFragment
                        };
                    }
                    else if (fragment is PaddingDivInputFragment paddingFragment)
                    {
                        control = new PaddingDivInputFragmentControl()
                        {
                            Fragment = paddingFragment
                        };
                    }
                    else if (fragment is RandomDivInputFragment randomFragment)
                    {
                        control = new RandomDivInputFragmentControl()
                        {
                            Fragment = randomFragment
                        };
                    }
                    else if (fragment is StaticDivInputFragment staticFragment)
                    {
                        control = new StaticDivInputFragmentControl()
                        {
                            Fragment = staticFragment
                        };
                    }

                    if (control != null)
                    {
                        var dialog = new DivInputFragmentDialog
                        {
                            DivInputFragmentControl = control
                        };
                        await DialogHelper.ForceShow(dialog, "fragmentDialog");
                    }
                }
            }
        }

        private void AddFragment_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFragmentType?.Clone() is DivInputFragment fragment)
            {
                DivInput.Add(fragment);
            }
        }
    }
}
