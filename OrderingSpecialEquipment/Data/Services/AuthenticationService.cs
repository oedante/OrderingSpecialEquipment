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

        public AuthenticationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User? AuthenticateCurrentUser()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null && !string.IsNullOrEmpty(windowsIdentity.Name))
            {
                // Ищем пользователя в БД по WindowsLogin
                // ВАЖНО: Это синхронный вызов асинхронного метода. В UI-потоке может вызвать блокировку.
                // В реальном приложении используйте асинхронный подход.
                _currentUser = _userRepository.GetByWindowsLoginAsync(windowsIdentity.Name).Result;
                // Проверяем, активен ли пользователь (проверка уже включена в репозиторий)
                // if (_currentUser != null && !_currentUser.IsActive)
                // {
                //     _currentUser = null; // Пользователь заблокирован
                // }
            }
            return _currentUser;
        }

        public bool IsUserAuthenticated => _currentUser != null;
    }
}