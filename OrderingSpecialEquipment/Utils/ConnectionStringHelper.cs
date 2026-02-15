using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace OrderingSpecialEquipment.Utils
{
    /// <summary>
    /// Вспомогательный класс для работы со строкой подключения
    /// </summary>
    public static class ConnectionStringHelper
    {
        #region Константы

        private const string CONFIG_FILE_NAME = "connection.dat";
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("OrderingSpecialEquipment");

        #endregion

        #region Публичные методы

        /// <summary>
        /// Сохранение строки подключения в зашифрованном виде
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        public static void SaveConnectionString(string connectionString)
        {
            try
            {
                string configPath = GetConfigPath();
                string directory = Path.GetDirectoryName(configPath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                byte[] connectionBytes = Encoding.UTF8.GetBytes(connectionString);
                byte[] encryptedData = ProtectedData.Protect(connectionBytes, Entropy, DataProtectionScope.CurrentUser);

                File.WriteAllBytes(configPath, encryptedData);

                System.Diagnostics.Debug.WriteLine($"Строка подключения сохранена в {configPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении строки подключения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загрузка строки подключения из зашифрованного файла
        /// </summary>
        /// <returns>Строка подключения или null, если файл не найден</returns>
        public static string LoadConnectionString()
        {
            try
            {
                string configPath = GetConfigPath();

                if (!File.Exists(configPath))
                {
                    return null;
                }

                byte[] encryptedData = File.ReadAllBytes(configPath);
                byte[] connectionBytes = ProtectedData.Unprotect(encryptedData, Entropy, DataProtectionScope.CurrentUser);

                return Encoding.UTF8.GetString(connectionBytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки строки подключения: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Проверка наличия сохраненной строки подключения
        /// </summary>
        public static bool HasConnectionString()
        {
            return File.Exists(GetConfigPath());
        }

        /// <summary>
        /// Удаление сохраненной строки подключения
        /// </summary>
        public static void ClearConnectionString()
        {
            try
            {
                string configPath = GetConfigPath();
                if (File.Exists(configPath))
                {
                    File.Delete(configPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при удалении строки подключения: {ex.Message}");
            }
        }

        /// <summary>
        /// Получение строки подключения по умолчанию
        /// </summary>
        public static string GetDefaultConnectionString()
        {
            return "Host=217.114.43.126;Port=5432;Database=OrderingSpecialEquipment;Username=student;Password=Qq587655!";
        }

        #endregion

        #region Приватные методы

        /// <summary>
        /// Получение пути к файлу конфигурации
        /// </summary>
        private static string GetConfigPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appDataPath, "OrderingSpecialEquipment", CONFIG_FILE_NAME);
        }

        #endregion
    }
}