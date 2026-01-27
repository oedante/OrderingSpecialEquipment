using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace OrderingSpecialEquipment.Converters
{
    /// <summary>
    /// Класс, содержащий все конвертеры, используемые в приложении.
    /// </summary>
    public static class Converters
    {
        // Пример реализации конкретного конвертера, если он нужен как отдельный класс
        // В данном случае, все конвертеры представлены как статические функции,
        // но для использования в XAML они оборачиваются в классы, наследующиеся от IValueConverter.
        // Ниже представлены эти обертки.

        /// <summary>
        /// Конвертер для отображения bool значения как строки "Да"/"Нет".
        /// </summary>
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

        /// <summary>
        /// Конвертер для отображения bool значения как строки "Активен"/"Не активен".
        /// </summary>
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

        /// <summary>
        /// Конвертер для отображения bool значения как Visibility.Collapsed/Visible.
        /// Если Inverted=true (через параметр), то инвертирует значение.
        /// </summary>
        public class BoolToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                bool b = value is bool && (bool)value;
                bool inverted = parameter is string param && param.ToLower() == "inverted";

                if (inverted)
                    b = !b;

                return b ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Конвертер для отображения bool значения как Visibility.Collapsed/Visible.
        /// Обратное поведение по сравнению с BoolToVisibilityConverter.
        /// </summary>
        public class BoolToVisibilityInvertedConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                bool b = value is bool && (bool)value;
                bool inverted = parameter is string param && param.ToLower() == "inverted";

                if (inverted)
                    b = !b;

                return b ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Конвертер для отображения числа как строки с нулями слева до заданной длины (через параметр).
        /// </summary>
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

        /// <summary>
        /// Конвертер для отображения DateTime как строки в формате "dd.MM.yyyy".
        /// </summary>
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

        /// <summary>
        /// Конвертер для отображения TimeSpan как строки в формате "HH:mm:ss".
        /// </summary>
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

        /// <summary>
        /// Конвертер для отображения объекта как его строкового представления (ToString).
        /// </summary>
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

        /// <summary>
        /// Конвертер для отображения Enum как строкового значения.
        /// </summary>
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

        /// <summary>
        /// Конвертер для отображения числа как денежной суммы (рубли).
        /// </summary>
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

        /// <summary>
        /// Конвертер, возвращающий null, если значение равно null или пустой строке.
        /// </summary>
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

        /// <summary>
        /// Конвертер, проверяющий, является ли значение null или пустой строкой.
        /// </summary>
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

        /// <summary>
        /// Конвертер, возвращающий значение из словаря по ключу.
        /// Параметр должен быть Dictionary<TKey, TValue>.
        /// </summary>
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

        // --- Примеры использования в XAML ---
        // xmlns:converters="clr-namespace:OrderingSpecialEquipment.Converters"
        //
        // <Window.Resources>
        //     <converters:BoolToYesNoConverter x:Key="BoolToYesNo"/>
        //     <converters:BoolToVisibilityConverter x:Key="BoolToVis"/>
        //     <converters:DateTimeToStringConverter x:Key="DateTimeToString"/>
        //     <!-- и т.д. -->
        // </Window.Resources>
        //
        // <TextBlock Text="{Binding SomeBoolProperty, Converter={StaticResource BoolToYesNo}}"/>
        // <TextBlock Visibility="{Binding SomeBoolProperty, Converter={StaticResource BoolToVis}, ConverterParameter=inverted}"/>

    }
}