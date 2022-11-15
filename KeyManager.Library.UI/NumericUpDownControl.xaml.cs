using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for NumericUpDownControl.xaml
    /// </summary>
    public partial class NumericUpDownControl : UserControl
    {
        public NumericUpDownControl()
        {
            InitializeComponent();
        }

        public string? Hint
        {
            get { return (string)GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }

        public static readonly DependencyProperty HintProperty = DependencyProperty.Register(nameof(Hint), typeof(string), typeof(NumericUpDownControl));

        public string? HelperText
        {
            get { return (string)GetValue(HelperTextProperty); }
            set { SetValue(HelperTextProperty, value); }
        }

        public static readonly DependencyProperty HelperTextProperty = DependencyProperty.Register(nameof(HelperText), typeof(string), typeof(NumericUpDownControl));

        public uint MinValue
        {
            get { return (uint)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(nameof(MinValue), typeof(uint), typeof(NumericUpDownControl),
            new FrameworkPropertyMetadata((uint)0));

        public uint MaxValue
        {
            get { return (uint)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(nameof(MaxValue), typeof(uint), typeof(NumericUpDownControl),
            new FrameworkPropertyMetadata((uint)100));

        public uint CurrentValue
        {
            get { return (uint)GetValue(CurrentValueProperty); }
            set
            {
                SetValue(CurrentValueProperty, value);
                NUDTextBox.Text = Convert.ToString(value);
            }
        }

        public static readonly DependencyProperty CurrentValueProperty = DependencyProperty.Register(nameof(CurrentValue), typeof(uint), typeof(NumericUpDownControl),
            new FrameworkPropertyMetadata((uint)0));

        private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
        {
            uint number;
            if (NUDTextBox.Text != "") number = Convert.ToUInt32(NUDTextBox.Text);
            else number = 0;
            if (number < MaxValue)
                CurrentValue = number + 1;
        }

        private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
        {
            uint number;
            if (NUDTextBox.Text != "") number = Convert.ToUInt32(NUDTextBox.Text);
            else number = 0;
            if (number > MinValue)
                CurrentValue = number - 1;
        }

        private void NUDTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == System.Windows.Input.Key.Up)
            {
                NUDButtonUP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(NUDButtonUP, new object[] { true });
            }


            if (e.Key == System.Windows.Input.Key.Down)
            {
                NUDButtonDown.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(NUDButtonDown, new object[] { true });
            }
        }

        private void NUDTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Up)
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(NUDButtonUP, new object[] { false });

            if (e.Key == System.Windows.Input.Key.Down)
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(NUDButtonDown, new object[] { false });
        }

        private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            uint number = 0;
            if (!string.IsNullOrEmpty(NUDTextBox.Text))
                if (!uint.TryParse(NUDTextBox.Text, out number)) NUDTextBox.Text = MinValue.ToString();
            if (number > MaxValue) NUDTextBox.Text = MaxValue.ToString();
            if (number < MinValue) NUDTextBox.Text = MinValue.ToString();
            NUDTextBox.SelectionStart = NUDTextBox.Text.Length;

            if (NUDTextBox.Text != Convert.ToString(CurrentValue))
            {
                CurrentValue = uint.Parse(NUDTextBox.Text);
            }
        }
    }
}
