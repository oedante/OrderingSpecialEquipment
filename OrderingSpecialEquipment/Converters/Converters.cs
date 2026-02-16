using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OrderingSpecialEquipment.Converters
{
    /// <summary>
    /// Конвертер Boolean в Visibility
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Конвертация Boolean в Visibility
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                bool invert = parameter != null && parameter.ToString() == "Inverted";

                if (invert)
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        /// Обратная конвертация (не реализована)
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер инверсии Boolean
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Инверсия Boolean
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;

            return false;
        }

        /// <summary>
        /// Обратная инверсия
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;

            return false;
        }
    }

    /// <summary>
    /// Конвертер смены (0/1) в текст
    /// </summary>
    [ValueConversion(typeof(int), typeof(string))]
    public class ShiftToTextConverter : IValueConverter
    {
        /// <summary>
        /// Конвертация кода смены в текст
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int shift)
            {
                return shift == 0 ? "Дневная" : "Ночная";
            }

            return "Неизвестно";
        }

        /// <summary>
        /// Конвертация текста в код смены
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string shiftText)
            {
                return shiftText == "Дневная" ? 0 : 1;
            }

            return 0;
        }
    }

    /// <summary>
    /// Конвертер Boolean в Opacity с поддержкой MultiBinding
    /// </summary>
    public class BooleanToOpacityConverter : IMultiValueConverter
    {
        /// <summary>
        /// Конвертация Boolean в Opacity
        /// </summary>
        /// <param name="values">Массив значений: [0] - Boolean, [1] - значение для True, [2] - значение для False</param>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.Length >= 1 && values[0] is bool boolValue)
                {
                    double trueValue = 1.0;
                    double falseValue = 0.3;

                    if (values.Length >= 2 && values[1] is double trueVal)
                    {
                        trueValue = trueVal;
                    }

                    if (values.Length >= 3 && values[2] is double falseVal)
                    {
                        falseValue = falseVal;
                    }

                    return boolValue ? trueValue : falseValue;
                }
            }
            catch
            {
                // В случае ошибки возвращаем значение по умолчанию
            }

            return 0.3;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер DateTime в строку даты
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class DateTimeToStringConverter : IValueConverter
    {
        /// <summary>
        /// Конвертация DateTime в строку
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                string format = parameter as string ?? "dd.MM.yyyy";
                return dateTime.ToString(format);
            }

            return string.Empty;
        }

        /// <summary>
        /// Конвертация строки в DateTime
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string dateString && !string.IsNullOrEmpty(dateString))
            {
                if (DateTime.TryParse(dateString, out DateTime result))
                    return result;
            }

            return DateTime.Today;
        }
    }

    /// <summary>
    /// Конвертер null или пустой строки в Visibility
    /// </summary>
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class NullOrEmptyToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Конвертация строки в Visibility
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter != null && parameter.ToString() == "Inverted";
            bool isEmpty = string.IsNullOrEmpty(value as string);

            if (invert)
                return isEmpty ? Visibility.Visible : Visibility.Collapsed;

            return isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Обратная конвертация (не реализована)
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер Decimal в строку с валютой
    /// </summary>
    [ValueConversion(typeof(decimal), typeof(string))]
    public class CurrencyConverter : IValueConverter
    {
        /// <summary>
        /// Конвертация Decimal в строку с валютой
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                return decimalValue.ToString("C2", CultureInfo.CurrentCulture);
            }

            return "0,00 ₽";
        }

        /// <summary>
        /// Конвертация строки в Decimal
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                string cleanValue = stringValue.Replace("₽", "").Replace(" ", "").Trim();
                if (decimal.TryParse(cleanValue, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal result))
                    return result;
            }

            return 0m;
        }
    }

    /// <summary>
    /// Конвертер для выделения цвета в зависимости от статуса заявки
    /// </summary>
    [ValueConversion(typeof(object), typeof(System.Windows.Media.Brush))]
    public class RequestStatusToBrushConverter : IValueConverter
    {
        private static readonly System.Windows.Media.SolidColorBrush WorkedBrush =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGreen);
        private static readonly System.Windows.Media.SolidColorBrush NotProvidedBrush =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightCoral);
        private static readonly System.Windows.Media.SolidColorBrush WeatherBrush =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue);
        private static readonly System.Windows.Media.SolidColorBrush BlockedBrush =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray);
        private static readonly System.Windows.Media.SolidColorBrush DefaultBrush =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent);

        /// <summary>
        /// Конвертация статуса заявки в цвет
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.ShiftRequest request)
            {
                if (request.IsBlocked)
                    return BlockedBrush;
                if (request.IsWorked)
                    return WorkedBrush;
                if (request.IsNotProvided)
                    return NotProvidedBrush;
                if (request.IsWeatherCancellation)
                    return WeatherBrush;
            }

            return DefaultBrush;
        }

        /// <summary>
        /// Обратная конвертация (не реализована)
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для мультиплексной привязки - объединение строк
    /// </summary>
    public class MultiValueStringConverter : IMultiValueConverter
    {
        /// <summary>
        /// Объединение нескольких значений в строку
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string separator = parameter as string ?? " ";
            return string.Join(separator, values);
        }

        /// <summary>
        /// Обратная конвертация (не реализована)
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер дня недели
    /// </summary>
    public class DayOfWeekConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                return date.DayOfWeek switch
                {
                    DayOfWeek.Monday => "Пн",
                    DayOfWeek.Tuesday => "Вт",
                    DayOfWeek.Wednesday => "Ср",
                    DayOfWeek.Thursday => "Чт",
                    DayOfWeek.Friday => "Пт",
                    DayOfWeek.Saturday => "Сб",
                    DayOfWeek.Sunday => "Вс",
                    _ => ""
                };
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер Boolean в цвет (зеленый/красный)
    /// </summary>
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Colors.Green : Colors.Red;
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер Boolean в текст "Да/Нет"
    /// </summary>
    public class BooleanToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Да" : "Нет";
            }
            return "Нет";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str == "Да";
            }
            return false;
        }
    }

    /// <summary>
    /// Конвертер Boolean в текст статуса подключения
    /// </summary>
    public class BooleanToStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Подключено" : "Отключено";
            }
            return "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер Boolean в текст (для двух вариантов)
    /// </summary>
    public class BooleanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string param)
            {
                var parts = param.Split(';');
                if (parts.Length == 2)
                {
                    return boolValue ? parts[0] : parts[1];
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для преобразования значения в видимость с проверкой на null
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter != null && parameter.ToString() == "Inverted";
            bool isNull = value == null;

            if (invert)
            {
                return isNull ? Visibility.Visible : Visibility.Collapsed;
            }
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для отображения суммы с валютой
    /// </summary>
    public class CurrencyFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                return decimalValue.ToString("N2") + " ₽";
            }
            if (value is double doubleValue)
            {
                return doubleValue.ToString("N2") + " ₽";
            }
            if (value is int intValue)
            {
                return intValue.ToString("N0") + " ₽";
            }
            return "0 ₽";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                string cleanStr = str.Replace("₽", "").Trim();
                if (decimal.TryParse(cleanStr, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal result))
                {
                    return result;
                }
            }
            return 0m;
        }
    }

    /// <summary>
    /// Конвертер для отображения процентов
    /// </summary>
    public class PercentFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                return decimalValue.ToString("F1") + "%";
            }
            if (value is double doubleValue)
            {
                return doubleValue.ToString("F1") + "%";
            }
            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для объединения двух значений в одну строку
    /// </summary>
    public class MultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return string.Empty;

            string format = parameter as string ?? "{0}";
            return string.Format(format, values);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для преобразования значения в цвет в зависимости от условия
    /// </summary>
    public class ConditionToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Colors.Black;

            string[] parts = parameter.ToString().Split('|');
            if (parts.Length >= 2)
            {
                string condition = parts[0];
                string colors = parts[1];

                string[] colorParts = colors.Split(',');
                if (colorParts.Length >= 2)
                {
                    if (value.ToString() == condition)
                    {
                        return (Color)ColorConverter.ConvertFromString(colorParts[0]);
                    }
                    else
                    {
                        return (Color)ColorConverter.ConvertFromString(colorParts[1]);
                    }
                }
            }

            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер TimeSpan в строку формата HH:mm
    /// </summary>
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan.ToString(@"hh\:mm");
            }
            return "00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && TimeSpan.TryParseExact(str, @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan result))
            {
                return result;
            }
            return TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Конвертер для группировки по дате
    /// </summary>
    public class DateGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is DateTime date)
            {
                return date.ToString("dd.MM.yyyy (dddd)", new System.Globalization.CultureInfo("ru-RU"));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}