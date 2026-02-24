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
    /// Элемент управления для выбора времени
    /// </summary>
    public partial class TimePicker : UserControl, INotifyPropertyChanged
    {
        #region События

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Поля

        private string _timeText = "07:30";
        private bool _isUpdating = false;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор TimePicker
        /// </summary>
        public TimePicker()
        {
            InitializeComponent();
            Value = TimeSpan.FromHours(8);
        }

        // Свойства для отображения ошибки
        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.Register("HasError", typeof(bool), typeof(TimePicker), new PropertyMetadata(false));

        public bool HasError
        {
            get => (bool)GetValue(HasErrorProperty);
            set => SetValue(HasErrorProperty, value);
        }

        public static readonly DependencyProperty ErrorTextProperty =
            DependencyProperty.Register("ErrorText", typeof(string), typeof(TimePicker), new PropertyMetadata(string.Empty));

        public string ErrorText
        {
            get => (string)GetValue(ErrorTextProperty);
            set => SetValue(ErrorTextProperty, value);
        }

        #endregion

        #region Свойства зависимостей

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(TimeSpan?), typeof(TimePicker),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged));

        #endregion

        #region Свойства

        /// <summary>
        /// Выбранное время
        /// </summary>
        public TimeSpan? Value
        {
            get { return (TimeSpan?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Текст для отображения
        /// </summary>
        public string TimeText
        {
            get => _timeText;
            set
            {
                if (_timeText != value)
                {
                    _timeText = value;
                    OnPropertyChanged();

                    if (!_isUpdating)
                    {
                        ParseTime();
                    }
                }
            }
        }

        #endregion

        #region Обработчики изменений

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TimePicker)d;
            control.UpdateTextFromValue();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Обновление текста из значения
        /// </summary>
        private void UpdateTextFromValue()
        {
            _isUpdating = true;
            if (Value.HasValue)
            {
                TimeText = Value.Value.ToString(@"hh\:mm");
            }
            else
            {
                TimeText = "00:00";
            }
            _isUpdating = false;
        }

        /// <summary>
        /// Парсинг времени из текста
        /// </summary>
        private void ParseTime()
        {
            if (string.IsNullOrWhiteSpace(TimeText))
            {
                Value = null;
                return;
            }

            string text = TimeText.Replace(" ", "").Replace(".", ":").Replace(",", ":");

            // Проверка формата HH:MM
            if (Regex.IsMatch(text, @"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$"))
            {
                if (TimeSpan.TryParseExact(text, @"hh\:mm", null, out TimeSpan result))
                {
                    Value = result;
                    _isUpdating = true;
                    TimeText = result.ToString(@"hh\:mm");
                    _isUpdating = false;
                    return;
                }
            }

            // Проверка формата H:MM
            if (Regex.IsMatch(text, @"^[0-9]:[0-5][0-9]$"))
            {
                if (TimeSpan.TryParseExact(text, @"h\:mm", null, out TimeSpan result))
                {
                    Value = result;
                    _isUpdating = true;
                    TimeText = result.ToString(@"hh\:mm");
                    _isUpdating = false;
                    return;
                }
            }

            // Если просто цифры - считаем часами
            if (int.TryParse(text, out int hours) && hours >= 0 && hours <= 23)
            {
                Value = TimeSpan.FromHours(hours);
                _isUpdating = true;
                TimeText = Value.Value.ToString(@"hh\:mm");
                _isUpdating = false;
                return;
            }

            // Если ничего не подошло, показываем ошибку и восстанавливаем предыдущее значение
            HasError = true;
            ErrorText = "Неверный формат времени. Ожидается HH:MM";
            if (Value.HasValue)
            {
                _isUpdating = true;
                TimeText = Value.Value.ToString(@"hh\:mm");
                _isUpdating = false;
            }
            else
            {
                _isUpdating = true;
                TimeText = "07:30";
                _isUpdating = false;
            }
        }

        #endregion

        #region Обработчики событий UI

        /// <summary>
        /// Предварительный ввод текста - разрешаем только цифры
        /// </summary>
        private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем цифры и ':'
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9:]$");
            if (!e.Handled)
            {
                // Сбрасываем ошибку при корректном вводе
                HasError = false;
                ErrorText = string.Empty;
            }
        }

        private void TimeTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            try
            {
                if (!e.SourceDataObject.GetDataPresent(DataFormats.Text))
                {
                    e.CancelCommand();
                    return;
                }

                var paste = e.SourceDataObject.GetData(DataFormats.Text) as string ?? string.Empty;
                var sanitized = Regex.Replace(paste, "[^0-9:]", string.Empty);
                if (sender is TextBox tb)
                {
                    int selStart = tb.SelectionStart;
                    int selLen = tb.SelectionLength;
                    var newText = tb.Text.Remove(selStart, selLen).Insert(selStart, sanitized);
                    if (newText.Length > 5)
                        newText = newText.Substring(0, 5);
                    tb.Text = newText;
                    tb.CaretIndex = Math.Min(selStart + sanitized.Length, tb.Text.Length);
                }
                e.CancelCommand();
                HasError = false;
                ErrorText = string.Empty;
            }
            catch
            {
                e.CancelCommand();
            }
        }

        /// <summary>
        /// Обработка нажатий клавиш
        /// </summary>
        private void TimeTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox)sender;

            // Разрешаем навигационные клавиши
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left ||
                e.Key == Key.Right || e.Key == Key.Tab || e.Key == Key.Enter ||
                e.Key == Key.Escape)
            {
                return;
            }

            // Ограничиваем длину ввода
            if (textBox.Text.Length >= 5 && e.Key != Key.Back && e.Key != Key.Delete)
            {
                e.Handled = true;
                return;
            }

            // Автоматическая вставка двоеточия после ввода двух цифр
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                string currentText = textBox.Text;
                int caretIndex = textBox.CaretIndex;

                // Если вводим первую цифру
                if (currentText.Length == 0)
                {
                    // Ничего не делаем, просто вводится цифра
                }
                // Если вводим вторую цифру и еще нет двоеточия
                else if (currentText.Length == 1 && !currentText.Contains(":"))
                {
                    // Добавим подсказку пользователю, но не меняем текст автоматически
                    // Это может сбивать пользователя
                }
                // Если уже есть двоеточие, просто вводим цифры
            }
        }

        /// <summary>
        /// Потеря фокуса - парсим время
        /// </summary>
        private void TimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ParseTime();
        }

        /// <summary>
        /// Увеличение времени на 30 минут
        /// </summary>
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

        /// <summary>
        /// Уменьшение времени на 30 минут
        /// </summary>
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

        #endregion

        #region INotifyPropertyChanged

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}