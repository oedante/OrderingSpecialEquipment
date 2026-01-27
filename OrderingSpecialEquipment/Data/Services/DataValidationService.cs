using System.Text.RegularExpressions;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация IDataValidationService.
    /// Основное внимание уделяется валидации форматов, а не сложной очистке строк.
    /// Защита от SQL-инъекций обеспечивается через использование параметризованных запросов в репозиториях.
    /// </summary>
    public class DataValidationService : IDataValidationService
    {
        // Простая очистка от базовых SQL-специфичных символов (не заменяет параметризованные запросы!)
        private static readonly Regex SqlInjectionPattern = new Regex(@"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE)?|INSERT|MERGE|SELECT|UPDATE|UNION( ALL)?)\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Удаление потенциальных команд SQL (очень базово)
            // Лучше использовать HtmlEncode и аналоги для вывода, и всегда параметризованные запросы.
            var sanitized = SqlInjectionPattern.Replace(input, "");
            // Также можно удалить другие потенциально опасные последовательности
            sanitized = sanitized.Replace("'", "''"); // Экранирование апострофа для SQL (частично)
            sanitized = sanitized.Replace("--", "");   // Комментарии в SQL
            sanitized = sanitized.Replace("/*", "");   // Многострочные комментарии
            sanitized = sanitized.Replace("*/", "");
            sanitized = sanitized.Replace(";", "");    // Терминатор команд
            return sanitized.Trim();
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                // Используем встроенный в .NET 5+ метод (или Regex для совместимости)
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            // Простая проверка на наличие только цифр, скобок, тире, пробелов и плюса
            return Regex.IsMatch(phone, @"^[+0-9\s\-\(\)]+$");
        }

        public bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            // Проверяем, что имя содержит только буквы, цифры, пробелы, дефисы, апострофы
            // и имеет разумную длину
            if (name.Length > 150) return false;
            return Regex.IsMatch(name, @"^[a-zA-Zа-яА-ЯёЁ0-9\s\-\'\.]+$");
        }

        public bool IsValidId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            // Проверяем формат ID: 2 буквы, затем 6 цифр (например, DE000001)
            return Regex.IsMatch(id, @"^[A-Z]{2}\d{6}$");
        }
    }
}