using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Интерфейс сервиса аутентификации
    /// Отвечает за проверку идентичности пользователя через Windows
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Аутентифицирует текущего пользователя через Windows
        /// </summary>
        Task<User?> AuthenticateCurrentUserAsync();

        /// <summary>
        /// Нормализует имя пользователя (удаляет домен)
        /// </summary>
        string NormalizeWindowsLogin(string windowsLogin);

        /// <summary>
        /// Текущий авторизованный пользователь
        /// </summary>
        User? CurrentUser { get; }
    }
}