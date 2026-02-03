using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OrderingSpecialEquipment.Converters
{
    /// <summary>
    /// Класс со всеми конвертерами приложения
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// Конвертер булевого значения в видимость
        /// </summary>
        public static readonly BoolToVisibilityConverter BoolToVisibilityConverter = new BoolToVisibilityConverter();

        /// <summary>
        /// Конвертер булевого значения в ширину (0 или заданное значение)
        /// </summary>
        public static readonly BoolToWidthConverter BoolToWidthConverter = new BoolToWidthConverter();

        /// <summary>
        /// Конвертер булевого значения в "Да/Нет"
        /// </summary>
        public static readonly BoolToYesNoConverter BoolToYesNoConverter = new BoolToYesNoConverter();

        /// <summary>
        /// Конвертер смены (0 - Ночная, 1 - Дневная)
        /// </summary>
        public static readonly ShiftConverter ShiftConverter = new ShiftConverter();

        /// <summary>
        /// Конвертер инвертирования булевого значения
        /// </summary>
        public static readonly InverseBooleanConverter InverseBooleanConverter = new InverseBooleanConverter();

        /// <summary>
        /// Конвертер нулевого значения в видимость
        /// </summary>
        public static readonly NullToVisibilityConverter NullToVisibilityConverter = new NullToVisibilityConverter();

        /// <summary>
        /// Конвертер форматирования даты
        /// </summary>
        public static readonly DateTimeFormatConverter DateTimeFormatConverter = new DateTimeFormatConverter();

        /// <summary>
        /// Конвертер форматирования чисел
        /// </summary>
        public static readonly NumberFormatConverter NumberFormatConverter = new NumberFormatConverter();

        /// <summary>
        /// Конвертер для форматирования валюты
        /// </summary>
        public static readonly CurrencyConverter CurrencyConverter = new CurrencyConverter();

        /// <summary>
        /// Конвертер для цвета текста в зависимости от фона
        /// </summary>
        public static readonly BackgroundToForegroundConverter BackgroundToForegroundConverter = new BackgroundToForegroundConverter();

        /// <summary>
        /// Конвертер для прав доступа (0-Запрещено, 1-Чтение, 2-Запись)
        /// </summary>
        public static readonly PermissionToBoolConverter PermissionToBoolConverter = new PermissionToBoolConverter();

        /// <summary>
        /// Конвертер для процента выполнения в цвет
        /// </summary>
        public static readonly CompletionToColorConverter CompletionToColorConverter = new CompletionToColorConverter();
    }

    /// <summary>
    /// Конвертер булевого значения в видимость
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Конвертер булевого значения в ширину (0 или заданное значение)
    /// </summary>
    public class BoolToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (boolValue)
                {
                    // Если параметр задан, используем его, иначе 300
                    if (parameter is string paramStr && double.TryParse(paramStr, out double width))
                    {
                        return width;
                    }
                    return 300.0;
                }
                return 0.0;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                return width > 0;
            }
            return false;
        }
    }

    /// <summary>
    /// Конвертер булевого значения в "Да/Нет"
    /// </summary>
    public class BoolToYesNoConverter : IValueConverter
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
            if (value is string strValue)
            {
                return strValue == "Да";
            }
            return false;
        }
    }

    /// <summary>
    /// Конвертер смены (0 - Ночная, 1 - Дневная)
    /// </summary>
    public class ShiftConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int shift)
            {
                return shift == 0 ? "Ночная" : "Дневная";
            }
            return "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                return strValue == "Ночная" ? 0 : 1;
            }
            return 0;
        }
    }

    /// <summary>
    /// Конвертер инвертирования булевого значения
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }
    }

    /// <summary>
    /// Конвертер нулевого значения в видимость
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            if (parameter is string paramStr && paramStr == "Inverse")
            {
                isNull = !isNull;
            }
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для цвета текста в зависимости от фона
    /// </summary>
    public class BackgroundToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                // Если фон темный, возвращаем светлый текст, и наоборот
                var color = brush.Color;
                double brightness = (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) / 255;
                return brightness < 0.5 ? Brushes.White : Brushes.Black;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для форматирования даты
    /// </summary>
    public class DateTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                string format = parameter as string ?? "d";
                return dateTime.ToString(format, culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                if (DateTime.TryParse(strValue, culture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }
            return DateTime.Now;
        }
    }

    /// <summary>
    /// Конвертер для форматирования чисел
    /// </summary>
    public class NumberFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                string format = parameter as string ?? "N2";
                return decimalValue.ToString(format, culture);
            }
            if (value is double doubleValue)
            {
                string format = parameter as string ?? "N2";
                return doubleValue.ToString(format, culture);
            }
            if (value is int intValue)
            {
                string format = parameter as string ?? "N0";
                return intValue.ToString(format, culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                if (decimal.TryParse(strValue, NumberStyles.Any, culture, out decimal result))
                {
                    return result;
                }
            }
            return 0m;
        }
    }

    /// <summary>
    /// Конвертер для форматирования валюты
    /// </summary>
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                return $"{decimalValue:N2} ₽";
            }

            if (value is double doubleValue)
            {
                return $"{doubleValue:N2} ₽";
            }

            if (value is int intValue)
            {
                return $"{intValue:N0} ₽";
            }

            if (value is long longValue)
            {
                return $"{longValue:N0} ₽";
            }

            return "0,00 ₽";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                var cleanValue = stringValue.Replace("₽", "")
                                            .Replace(" ", "")
                                            .Replace(",", ".");

                if (decimal.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                {
                    return result;
                }
            }

            return 0m;
        }
    }

    /// <summary>
    /// Конвертер для прав доступа (0-Запрещено, 1-Чтение, 2-Запись)
    /// </summary>
    public class PermissionToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is short permissionLevel)
            {
                return permissionLevel > 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? (short)2 : (short)0; // Запись или запрещено
            }
            return (short)0;
        }
    }

    /// <summary>
    /// Конвертер для процента выполнения в цвет
    /// </summary>
    public class CompletionToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal percentage)
            {
                // Если больше 100%, зеленый (перевыполнение)
                if (percentage >= 100)
                    return Brushes.Green;
                // Если больше 90%, зеленоватый
                else if (percentage >= 90)
                    return Brushes.LightGreen;
                // Если больше 80%, желтый
                else if (percentage >= 80)
                    return Brushes.Orange;
                // Если меньше 80%, красный
                else
                    return Brushes.Red;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}