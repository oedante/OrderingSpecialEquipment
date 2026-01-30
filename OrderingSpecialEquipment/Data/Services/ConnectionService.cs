using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация IConnectionService для управления строкой подключения к базе данных.
    /// </summary>
    public class ConnectionService : IConnectionService
    {
        private readonly IConfiguration _configuration;
        private const string DefaultConnectionString = "Host=217.114.43.126;Port=5432;Database=OrderingSpecialEquipment;Username=student;Password=Qq587655!;"; // Заглушка
        private const ConnectionType DefaultConnectionType = ConnectionType.PostgreSQL;

        public ConnectionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ConnectionType CurrentConnectionType
        {
            get
            {
                var typeStr = _configuration["ConnectionType"];
                if (Enum.TryParse<ConnectionType>(typeStr, out var type))
                {
                    return type;
                }
                // Возвращаем значение по умолчанию, если в конфигурации нет или ошибка
                return DefaultConnectionType;
            }
        }

        public string GetConnectionString()
        {
            var cs = _configuration.GetConnectionString("DefaultConnection");
            // Возвращаем строку из конфигурации или заглушку, если её нет
            return string.IsNullOrEmpty(cs) ? DefaultConnectionString : cs;
        }

        public bool ValidateConnectionString(string connectionString)
        {
            // Простая проверка на null/empty. Более сложная проверка - через тест подключения.
            return !string.IsNullOrEmpty(connectionString) && connectionString.Length > 5; // Пример минимальной длины
        }

        public bool TestConnection(string connectionString)
        {
            // Тестирование подключения к БД (реализация зависит от провайдера)
            // В реальном приложении используйте NpgsqlConnection.Open() или SqlConnection.Open()
            try
            {
                if (!ValidateConnectionString(connectionString)) return false;

                using var connection = CreateConnection(connectionString, CurrentConnectionType);
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SaveConnectionSettings(ConnectionType connectionType, string connectionString)
        {
            // Сохранение в файл настроек (appsettings.json или отдельный файл)
            // Для упрощения, предположим, что настройки хранятся в appsettings.json
            // Это требует использования IConfigurationRoot и перезаписи файла, что не так тривиально.
            // В реальном приложении может использоваться отдельный JSON-файл для настроек подключения.
            // Пока оставим как заглушку.
            Console.WriteLine($"Попытка сохранить настройки подключения: {connectionType}, {connectionString}");
            //throw new NotImplementedException("Сохранение настроек подключения не реализовано в этом примере.");
        }

        public (ConnectionType type, string connectionString) LoadConnectionSettings()
        {
            // Загрузка из appsettings.json (или другого источника)
            // Это уже делает IConfiguration
            var cs = GetConnectionString();
            var type = CurrentConnectionType;
            return (type, cs);
        }

        // Вспомогательный метод для создания соединения (требует соответствующие NuGet-пакеты)
        private System.Data.Common.DbConnection CreateConnection(string connectionString, ConnectionType type)
        {
            switch (type)
            {
                case ConnectionType.PostgreSQL:
                    // Убедитесь, что установлен пакет Npgsql
                    return new Npgsql.NpgsqlConnection(connectionString);
                case ConnectionType.MSSQL:
                    // Убедитесь, что установлен пакет Microsoft.Data.SqlClient
                    return new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                default:
                    throw new InvalidOperationException($"Unsupported connection type: {type}");
            }
        }
    }
}