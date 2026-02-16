using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OrderingSpecialEquipment.Controls
{
    public partial class TimePicker : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TimePicker()
        {
            InitializeComponent();
            TimeText = "08:00";
            Value = TimeSpan.FromHours(8);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(TimeSpan?), typeof(TimePicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public TimeSpan? Value
        {
            get { return (TimeSpan?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private string _timeText = "08:00";
        public string TimeText
        {
            get => _timeText;
            set
            {
                if (_timeText != value)
                {
                    _timeText = value;
                    OnPropertyChanged();
                }
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TimePicker)d;
            if (e.NewValue is TimeSpan timeSpan)
            {
                control.TimeText = timeSpan.ToString(@"hh\:mm");
            }
        }

        private void TimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ParseTime();
        }

        private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры и двоеточие
            var regex = new Regex(@"^[0-9:]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void TimeTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Автоматическая вставка двоеточия после ввода двух цифр
            if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                var textBox = (TextBox)sender;
                if (textBox.Text.Length == 2 && !textBox.Text.Contains(":"))
                {
                    textBox.Text += ":";
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        private void ParseTime()
        {
            if (string.IsNullOrWhiteSpace(TimeText))
            {
                Value = TimeSpan.FromHours(8);
                TimeText = "08:00";
                return;
            }

            string text = TimeText.Replace(" ", "").Replace(".", ":").Replace(",", ":");

            if (TimeSpan.TryParseExact(text, @"hh\:mm", null, out TimeSpan result))
            {
                Value = result;
                TimeText = result.ToString(@"hh\:mm");
            }
            else if (TimeSpan.TryParseExact(text, @"h\:mm", null, out result))
            {
                Value = result;
                TimeText = result.ToString(@"hh\:mm");
            }
            else if (int.TryParse(text, out int hours) && hours >= 0 && hours <= 23)
            {
                Value = TimeSpan.FromHours(hours);
                TimeText = Value.Value.ToString(@"hh\:mm");
            }
            else
            {
                Value = TimeSpan.FromHours(8);
                TimeText = "08:00";
            }
        }

        private void IncreaseHour_Click(object sender, RoutedEventArgs e)
        {
            if (Value.HasValue)
            {
                Value = Value.Value.Add(TimeSpan.FromMinutes(30));
                if (Value.Value.TotalHours >= 24)
                    Value = TimeSpan.FromHours(0);
            }
            else
            {
                Value = TimeSpan.FromHours(8);
            }
        }

        private void DecreaseHour_Click(object sender, RoutedEventArgs e)
        {
            if (Value.HasValue)
            {
                Value = Value.Value.Subtract(TimeSpan.FromMinutes(30));
                if (Value.Value.TotalHours < 0)
                    Value = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30));
            }
            else
            {
                Value = TimeSpan.FromHours(8);
            }
        }
    }
}