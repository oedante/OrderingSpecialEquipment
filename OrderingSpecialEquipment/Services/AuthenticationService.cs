using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Сервис аутентификации пользователей через Windows
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        #region Поля

        private readonly IDatabaseService _databaseService;
        private User _currentUser;
        private Role _currentUserRole;

        #endregion

        #region Свойства

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public User CurrentUser => _currentUser;

        /// <summary>
        /// Роль текущего пользователя
        /// </summary>
        public Role CurrentUserRole => _currentUserRole;

        /// <summary>
        /// Выполнен ли вход
        /// </summary>
        public bool IsAuthenticated => _currentUser != null;

        #endregion

        #region События

        /// <summary>
        /// Событие изменения пользователя
        /// </summary>
        public event EventHandler<User> UserChanged;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор сервиса аутентификации
        /// </summary>
        /// <param name="databaseService">Сервис БД</param>
        public AuthenticationService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Попытка аутентификации текущего пользователя Windows
        /// </summary>
        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                // Проверяем подключение к БД
                if (!_databaseService.IsConnected)
                {
                    System.Diagnostics.Debug.WriteLine("Аутентификация невозможна: нет подключения к БД");
                    return false;
                }

                if (_databaseService.Context == null)
                {
                    System.Diagnostics.Debug.WriteLine("Аутентификация невозможна: контекст БД не инициализирован");
                    return false;
                }

                // Получаем логин текущего пользователя Windows
                string windowsLogin = GetCurrentWindowsLogin();
                if (string.IsNullOrEmpty(windowsLogin))
                {
                    System.Diagnostics.Debug.WriteLine("Не удалось получить Windows логин");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Попытка аутентификации пользователя: {windowsLogin}");

                // Ищем пользователя в БД
                var user = await _databaseService.Context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.WindowsLogin == windowsLogin && u.IsActive);

                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Пользователь {windowsLogin} не найден в БД");

                    // Для отладки покажем всех пользователей
                    var allUsers = await _databaseService.Context.Users.ToListAsync();
                    System.Diagnostics.Debug.WriteLine($"Всего пользователей в БД: {allUsers.Count}");
                    foreach (var u in allUsers)
                    {
                        System.Diagnostics.Debug.WriteLine($" - {u.WindowsLogin} ({u.FullName})");
                    }

                    return false;
                }

                // Устанавливаем текущего пользователя
                _currentUser = user;
                _currentUserRole = user.Role;

                System.Diagnostics.Debug.WriteLine($"Пользователь {user.FullName} успешно аутентифицирован");

                // Вызываем событие
                UserChanged?.Invoke(this, _currentUser);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка аутентификации: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Выход из системы
        /// </summary>
        public void Logout()
        {
            _currentUser = null;
            _currentUserRole = null;
            UserChanged?.Invoke(this, null);
        }

        #endregion

        #region Приватные методы

        /// <summary>
        /// Получение логина текущего пользователя Windows
        /// </summary>
        private string GetCurrentWindowsLogin()
        {
            try
            {
                var windowsIdentity = WindowsIdentity.GetCurrent();
                if (windowsIdentity != null)
                {
                    string fullLogin = windowsIdentity.Name;

                    // Нормализация: удаляем домен, оставляем только имя пользователя
                    if (fullLogin.Contains("\\"))
                    {
                        return fullLogin.Substring(fullLogin.IndexOf("\\") + 1);
                    }

                    return fullLogin;
                }

                // Для тестирования без Windows
                string userName = Environment.UserName;
                System.Diagnostics.Debug.WriteLine($"Используется Environment.UserName: {userName}");
                return userName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения Windows логина: {ex.Message}");
                return Environment.UserName;
            }
        }

        #endregion
    }
}