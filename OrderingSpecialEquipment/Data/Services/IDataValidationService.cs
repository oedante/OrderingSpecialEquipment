using System.Text.RegularExpressions;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Интерфейс для сервиса валидации и очистки данных, защищающего от SQL-инъекций.
    /// </summary>
    public interface IDataValidationService
    {
        /// <summary>
        /// Очищает строку от потенциально опасных символов, связанных с SQL-инъекцией.
        /// ВНИМАНИЕ: Это не основной способ защиты! Защита через параметризованные запросы важнее.
        /// </summary>
        /// <param name="input">Входная строка.</param>
        /// <returns>Очищенная строка.</returns>
        string SanitizeInput(string input);

        /// <summary>
        /// Проверяет, соответствует ли строка формату электронной почты.
        /// </summary>
        /// <param name="email">Строка для проверки.</param>
        /// <returns>True, если формат действителен.</returns>
        bool IsValidEmail(string email);

        /// <summary>
        /// Проверяет, соответствует ли строка формату телефонного номера.
        /// </summary>
        /// <param name="phone">Строка для проверки.</param>
        /// <returns>True, если формат действителен.</returns>
        bool IsValidPhone(string phone);

        /// <summary>
        /// Проверяет, является ли строка допустимым именем или наименованием (без недопустимых символов).
        /// </summary>
        /// <param name="name">Строка для проверки.</param>
        /// <returns>True, если имя допустимо.</returns>
        bool IsValidName(string name);

        /// <summary>
        /// Проверяет, является ли строка допустимым ID (например, формату DE000001).
        /// </summary>
        /// <param name="id">Строка ID для проверки.</param>
        /// <returns>True, если ID допустим.</returns>
        bool IsValidId(string id);
    }
}