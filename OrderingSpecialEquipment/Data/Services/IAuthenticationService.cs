using OrderingSpecialEquipment.Models;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Интерфейс для сервиса аутентификации пользователей по логину Windows.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Пытается аутентифицировать пользователя по текущему логину Windows.
        /// </summary>
        /// <returns>Модель User, если аутентификация успешна, иначе null.</returns>
        User? AuthenticateCurrentUser();

        /// <summary>
        /// Проверяет, аутентифицирован ли пользователь.
        /// </summary>
        /// <returns>True, если пользователь аутентифицирован.</returns>
        bool IsUserAuthenticated { get; }
    }
}