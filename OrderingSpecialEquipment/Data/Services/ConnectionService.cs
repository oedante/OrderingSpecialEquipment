using Npgsql; // Npgsql
using System;
using System.Data.SqlClient; // Microsoft.Data.SqlClient
using System.Data.Common;
using System.IO;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация IConnectionService для работы с PostgreSQL и MSSQL.
    /// </summary>
    public class ConnectionService : IConnectionService
    {
        private const string SettingsFileName = "DbConnectionSettings.json";
        private readonly string _settingsFilePath;

        public ConnectionType CurrentConnectionType { get; private set; } = ConnectionType.PostgreSQL; // Значение по умолчанию

        public ConnectionService()
        {
            _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
            // Загрузка настроек при создании сервиса
            var loadedSettings = LoadConnectionSettings();
            CurrentConnectionType = loadedSettings.type;
        }

        public string GetConnectionString()
        {
            var settings = LoadConnectionSettings();
            return settings.connectionString;
        }

        public bool ValidateConnectionString(string connectionString)
        {
            try
            {
                switch (CurrentConnectionType)
                {
                    case ConnectionType.PostgreSQL:
                        using (var conn = new NpgsqlConnection(connectionString)) { /* conn.ConnectionString parsing logic could be added here */ }
                        break;
                    case ConnectionType.MSSQL:
                        using (var conn = new SqlConnection(connectionString)) { /* conn.ConnectionString parsing logic could be added here */ }
                        break;
                    default:
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TestConnection(string connectionString)
        {
            try
            {
                DbConnection connection;
                switch (CurrentConnectionType)
                {
                    case ConnectionType.PostgreSQL:
                        connection = new NpgsqlConnection(connectionString);
                        break;
                    case ConnectionType.MSSQL:
                        connection = new SqlConnection(connectionString);
                        break;
                    default:
                        return false;
                }

                connection.Open();
                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка тестирования подключения: {ex.Message}");
                return false;
            }
        }

        public void SaveConnectionSettings(ConnectionType connectionType, string connectionString)
        {
            try
            {
                var settings = new ConnectionSettings
                {
                    Type = connectionType.ToString(),
                    ConnectionString = connectionString
                };
                var json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
                Console.WriteLine("Настройки подключения сохранены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения настроек подключения: {ex.Message}");
            }
        }

        public (ConnectionType type, string connectionString) LoadConnectionSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = System.Text.Json.JsonSerializer.Deserialize<ConnectionSettings>(json);
                    if (settings != null)
                    {
                        var type = Enum.TryParse<ConnectionType>(settings.Type, out var parsedType) ? parsedType : ConnectionType.PostgreSQL;
                        return (type, settings.ConnectionString);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки настроек подключения: {ex.Message}");
            }
            // Значения по умолчанию
            return (ConnectionType.PostgreSQL, "");
        }

        // Вспомогательный класс для сериализации/десериализации настроек
        private class ConnectionSettings
        {
            public string Type { get; set; } = ConnectionType.PostgreSQL.ToString();
            public string ConnectionString { get; set; } = "";
        }
    }
}