using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Models;
using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация сервиса аутентификации
    /// Использует WindowsIdentity для получения текущего пользователя
    /// Создает новый IServiceScope для работы с репозиторием
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IServiceProvider _serviceProvider;
        private User? _cachedUser;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public AuthenticationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Текущий авторизованный пользователь (кэшируется после первой аутентификации)
        /// </summary>
        public User? CurrentUser => _cachedUser;

        /// <summary>
        /// Аутентифицирует текущего пользователя через Windows
        /// </summary>
        public async Task<User?> AuthenticateCurrentUserAsync()
        {
            // Если пользователь уже аутентифицирован, возвращаем кэшированного
            if (_cachedUser != null)
                return _cachedUser;

            try
            {
                // Получаем текущего пользователя Windows
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                string windowsLogin = identity.Name;

                // Нормализуем логин (удаляем домен)
                string normalizedLogin = NormalizeWindowsLogin(windowsLogin);

                // Создаем новый сервис скоуп для работы с репозиторием
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    // Ищем пользователя в базе данных
                    User? user = await userRepository.GetByWindowsLoginAsync(normalizedLogin);

                    if (user == null)
                    {
                        Console.WriteLine($"Пользователь {normalizedLogin} не найден в базе данных");
                    }
                    else
                    {
                        _cachedUser = user;
                    }

                    return user;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Ошибка аутентификации: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Нормализует имя пользователя Windows
        /// Удаляет домен из имени (DOMAIN\username -> username)
        /// </summary>
        public string NormalizeWindowsLogin(string windowsLogin)
        {
            if (string.IsNullOrWhiteSpace(windowsLogin))
                return string.Empty;

            // Разделяем по обратному слешу и берем последнюю часть
            string[] parts = windowsLogin.Split('\\');
            return parts.Length > 1 ? parts[1] : windowsLogin;
        }
    }
}