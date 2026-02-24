using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OrderingSpecialEquipment.Controls
{
    /// <summary>
    /// Логика взаимодействия для NumericTextBox.xaml
    /// </summary>
    public partial class NumericTextBox : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public NumericTextBox()
        {
            InitializeComponent();
            this.DataContext = this;
            TextValue = Value.ToString();
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumericTextBox),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(int), typeof(NumericTextBox),
                new PropertyMetadata(1));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int), typeof(NumericTextBox),
                new PropertyMetadata(int.MaxValue));

        public static readonly DependencyProperty ControlIsEnabledProperty =
            DependencyProperty.Register("ControlIsEnabled", typeof(bool), typeof(NumericTextBox),
                new PropertyMetadata(true, OnControlIsEnabledChanged));

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public bool ControlIsEnabled
        {
            get { return (bool)GetValue(ControlIsEnabledProperty); }
            set { SetValue(ControlIsEnabledProperty, value); }
        }

        private string _textValue = "1";
        public string TextValue
        {
            get => _textValue;
            set
            {
                if (_textValue != value)
                {
                    _textValue = value;
                    OnPropertyChanged();
                }
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericTextBox)d;
            control.TextValue = e.NewValue?.ToString() ?? "1";
        }

        private static void OnControlIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericTextBox)d;
            control.ValueTextBox.IsEnabled = (bool)e.NewValue;
        }

        private void ValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            var regex = new Regex(@"^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ParseValue();
        }

        private void ParseValue()
        {
            if (int.TryParse(TextValue, out int newValue))
            {
                if (newValue < MinValue) newValue = MinValue;
                if (newValue > MaxValue) newValue = MaxValue;
                Value = newValue;
            }
            else
            {
                // Неверный формат, возвращаем текущее значение
                TextValue = Value.ToString();
            }
        }

        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            if (ControlIsEnabled && Value < MaxValue)
            {
                Value++;
            }
        }

        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            if (ControlIsEnabled && Value > MinValue)
            {
                Value--;
            }
        }
    }
}