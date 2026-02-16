using OrderingSpecialEquipment.Models;

namespace OrderingSpecialEquipment.Services.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса аутентификации
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Текущий пользователь
        /// </summary>
        User? CurrentUser { get; }

        /// <summary>
        /// Роль текущего пользователя
        /// </summary>
        Role? CurrentUserRole { get; }

        /// <summary>
        /// Выполнен ли вход
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Попытка аутентификации текущего пользователя Windows
        /// </summary>
        Task<bool> AuthenticateAsync();

        /// <summary>
        /// Выход из системы
        /// </summary>
        void Logout();

        /// <summary>
        /// Событие изменения пользователя
        /// </summary>
        event EventHandler<User?> UserChanged;
    }
}