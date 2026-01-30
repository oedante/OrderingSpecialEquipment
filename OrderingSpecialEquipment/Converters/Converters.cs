using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OrderingSpecialEquipment.Converters
{
    // Каждый конвертер теперь отдельный public class
    public class BoolToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? "Да" : "Нет";
            }
            return "Нет";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                return s == "Да";
            }
            return false;
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value is bool && (bool)value;
            bool inverted = parameter is string param && param.ToLower() == "inverted";

            if (inverted)
                b = !b;

            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToActiveInactiveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? "Активен" : "Не активен";
            }
            return "Не активен";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                return s == "Активен";
            }
            return false;
        }
    }

    public class IntToStringWithPaddingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                int length = 3; // Значение по умолчанию
                if (parameter != null && int.TryParse(parameter.ToString(), out int paramLength))
                {
                    length = paramLength;
                }
                return i.ToString($"D{length}");
            }
            return "000";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && int.TryParse(s, out int result))
            {
                return result;
            }
            return 0;
        }
    }

    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                return dt.ToString("dd.MM.yyyy");
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && DateTime.TryParseExact(s, "dd.MM.yyyy", culture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            return DateTime.Now;
        }
    }

    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
            {
                return ts.ToString(@"hh\:mm\:ss");
            }
            return "00:00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && TimeSpan.TryParse(s, out TimeSpan result))
            {
                return result;
            }
            return TimeSpan.Zero;
        }
    }

    public class ObjectToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConvertBack для этого конвертера обычно не имеет смысла
            throw new NotImplementedException();
        }
    }

    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return value.ToString();
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && targetType.IsEnum)
            {
                return Enum.Parse(targetType, s);
            }
            throw new ArgumentException("Target type must be an enum and value must be a string.");
        }
    }

    public class DecimalToCurrencyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d)
            {
                // Используем культуру по умолчанию или переданную
                var info = culture ?? CultureInfo.CurrentCulture;
                return d.ToString("C2", info); // "C2" - валюта с 2 знаками после запятой
            }
            return "0.00 ₽";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                // Пытаемся извлечь число из строки валюты
                var numberStyle = NumberStyles.Currency;
                var info = culture ?? CultureInfo.CurrentCulture;
                if (decimal.TryParse(s, numberStyle, info, out decimal result))
                {
                    return result;
                }
            }
            return 0m;
        }
    }

    public class NullOrEmptyToNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return string.IsNullOrEmpty(str) ? null : value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class IsNullOrEmptyToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return string.IsNullOrEmpty(str);
            }
            return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DictionaryLookupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Binding.DoNothing;

            var dict = parameter as System.Collections.IDictionary;
            if (dict != null && dict.Contains(value))
            {
                return dict[value];
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible && isVisible)
            {
                if (parameter is double width)
                {
                    return new GridLength(width);
                }
                return new GridLength(300); // Значение по умолчанию
            }
            return new GridLength(0); // Сворачиваем колонку
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConvertBack не имеет смысла для ширины колонки
            throw new NotImplementedException();
        }
    }

}