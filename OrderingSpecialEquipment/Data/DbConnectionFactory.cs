using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Data.SqlClient;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Фабрика для создания подключений к БД
    /// </summary>
    public static class DbConnectionFactory
    {
        /// <summary>
        /// Тип базы данных
        /// </summary>
        public enum DatabaseType
        {
            PostgreSQL,
            SqlServer
        }

        /// <summary>
        /// Создание опций DbContext на основе строки подключения
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <param name="dbType">Тип БД</param>
        public static DbContextOptions<ApplicationDbContext> CreateDbContextOptions(string connectionString, DatabaseType dbType)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            switch (dbType)
            {
                case DatabaseType.PostgreSQL:
                    // Настройка для PostgreSQL с правильной обработкой DateTime
                    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                    AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

                    optionsBuilder.UseNpgsql(connectionString, options =>
                    {
                        options.CommandTimeout(60);
                        options.EnableRetryOnFailure(3);
                    });
                    break;

                case DatabaseType.SqlServer:
                    optionsBuilder.UseSqlServer(connectionString, options =>
                    {
                        options.CommandTimeout(60);
                        options.EnableRetryOnFailure(3);
                    });
                    break;

                default:
                    throw new ArgumentException($"Неподдерживаемый тип БД: {dbType}");
            }

            return optionsBuilder.Options;
        }

        /// <summary>
        /// Проверка подключения к БД
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <param name="dbType">Тип БД</param>
        public static bool TestConnection(string connectionString, DatabaseType dbType)
        {
            try
            {
                switch (dbType)
                {
                    case DatabaseType.PostgreSQL:
                        using (var connection = new NpgsqlConnection(connectionString))
                        {
                            connection.Open();
                            return true;
                        }

                    case DatabaseType.SqlServer:
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            return true;
                        }

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка проверки подключения: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Получение типа БД из строки подключения
        /// </summary>
        public static DatabaseType? DetectDatabaseType(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return null;

            if (connectionString.Contains("Host=") ||
                (connectionString.Contains("Database=") && connectionString.Contains("Username=")))
                return DatabaseType.PostgreSQL;

            if (connectionString.Contains("Server=") || connectionString.Contains("Initial Catalog="))
                return DatabaseType.SqlServer;

            return null;
        }

        /// <summary>
        /// Формирование строки подключения для PostgreSQL из параметров
        /// </summary>
        public static string BuildPostgreSqlConnectionString(string host, string port, string database, string username, string password)
        {
            return $"Host={host};Port={port};Database={database};Username={username};Password={password};";
        }

        /// <summary>
        /// Формирование строки подключения для SQL Server из параметров
        /// </summary>
        public static string BuildSqlServerConnectionString(string server, string database, string username, string password)
        {
            return $"Server={server};Database={database};User Id={username};Password={password};TrustServerCertificate=True;";
        }
    }
}