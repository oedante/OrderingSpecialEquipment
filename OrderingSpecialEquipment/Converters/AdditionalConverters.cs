using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OrderingSpecialEquipment.Converters
{
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
}