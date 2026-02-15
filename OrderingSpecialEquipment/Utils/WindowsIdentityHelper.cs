using System.Security.Principal;

namespace OrderingSpecialEquipment.Utils
{
    /// <summary>
    /// Вспомогательный класс для работы с Windows Identity
    /// </summary>
    public static class WindowsIdentityHelper
    {
        /// <summary>
        /// Получение нормализованного Windows логина текущего пользователя
        /// </summary>
        /// <returns>Логин без домена</returns>
        public static string GetCurrentWindowsLogin()
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
            }
            catch
            {
                // Игнорируем ошибки
            }

            // Возвращаем имя пользователя окружения как запасной вариант
            return System.Environment.UserName;
        }

        /// <summary>
        /// Проверка, является ли текущий пользователь администратором
        /// </summary>
        public static bool IsCurrentUserAdministrator()
        {
            try
            {
                var windowsIdentity = WindowsIdentity.GetCurrent();
                if (windowsIdentity != null)
                {
                    var principal = new WindowsPrincipal(windowsIdentity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                // Игнорируем ошибки
            }

            return false;
        }

        /// <summary>
        /// Получение полного имени пользователя из Windows
        /// </summary>
        public static string GetCurrentUserFullName()
        {
            try
            {
                var windowsIdentity = WindowsIdentity.GetCurrent();
                if (windowsIdentity != null)
                {
                    // В реальном проекте здесь можно получить имя из Active Directory
                    // Для простоты возвращаем логин
                    return windowsIdentity.Name;
                }
            }
            catch
            {
                // Игнорируем ошибки
            }

            return System.Environment.UserName;
        }
    }
}