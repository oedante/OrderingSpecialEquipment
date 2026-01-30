using OrderingSpecialEquipment.Data.Repositories;
using OrderingSpecialEquipment.Models;
using System.Security.Principal;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация IAuthenticationService для аутентификации через Windows Login.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private User? _currentUser;
        private readonly IUserRepository _userRepository; // Зависимость от репозитория

        public AuthenticationService(IUserRepository userRepository) // Принимаем IUserRepository через DI
        {
            _userRepository = userRepository;
        }

        public User? AuthenticateCurrentUser()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null && !string.IsNullOrEmpty(windowsIdentity.Name))
            {
                // --- НОРМАЛИЗАЦИЯ WINDOWS LOGIN ---
                string normalizedWindowsLogin = NormalizeWindowsLogin(windowsIdentity.Name);

                // Ищем пользователя в БД по нормализованному логину Windows
                _currentUser = _userRepository.GetByWindowsLoginAsync(normalizedWindowsLogin).Result; // Используем Result осторожно
                // Проверяем, активен ли пользователь (проверка уже включена в репозитории, если там добавлено && u.IsActive)
                if (_currentUser != null && !_currentUser.IsActive)
                {
                    _currentUser = null; // Пользователь заблокирован
                }
            }
            return _currentUser;
        }

        public bool IsUserAuthenticated => _currentUser != null;

        // --- ВСПОМОГАТЕЛЬНЫЙ МЕТОД ---
        /// <summary>
        /// Нормализует логин Windows, удаляя домен (DOMAIN\username -> username).
        /// </summary>
        /// <param name="fullLogin">Полный логин Windows (DOMAIN\username или username).</param>
        /// <returns>Нормализованный логин (username).</returns>
        private static string NormalizeWindowsLogin(string fullLogin)
        {
            if (string.IsNullOrEmpty(fullLogin))
                return fullLogin;

            var parts = fullLogin.Split('\\');
            if (parts.Length > 1)
            {
                return parts[parts.Length - 1];
            }
            return fullLogin;
        }
    }
}