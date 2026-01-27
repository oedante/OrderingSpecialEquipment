using System;
using System.Data.Common;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Интерфейс для сервиса управления строкой подключения к базе данных.
    /// Позволяет переключаться между PostgreSQL и MS SQL.
    /// </summary>
    public interface IConnectionService
    {
        /// <summary>
        /// Тип подключения: PostgreSQL или MSSQL.
        /// </summary>
        ConnectionType CurrentConnectionType { get; }

        /// <summary>
        /// Получает строку подключения на основе сохраненных настроек.
        /// </summary>
        /// <returns>Строка подключения.</returns>
        string GetConnectionString();

        /// <summary>
        /// Проверяет, является ли строка подключения корректной для текущего типа.
        /// </summary>
        /// <param name="connectionString">Строка подключения для проверки.</param>
        /// <returns>True, если строка валидна, иначе false.</returns>
        bool ValidateConnectionString(string connectionString);

        /// <summary>
        /// Тестирует подключение к базе данных.
        /// </summary>
        /// <param name="connectionString">Строка подключения для тестирования.</param>
        /// <returns>True, если подключение успешно, иначе false.</returns>
        bool TestConnection(string connectionString);

        /// <summary>
        /// Сохраняет настройки подключения (тип, строку, параметры) в файл настроек.
        /// </summary>
        /// <param name="connectionType">Тип подключения.</param>
        /// <param name="connectionString">Строка подключения.</param>
        void SaveConnectionSettings(ConnectionType connectionType, string connectionString);

        /// <summary>
        /// Загружает настройки подключения из файла настроек.
        /// </summary>
        /// <returns>Тип подключения и строка подключения.</returns>
        (ConnectionType type, string connectionString) LoadConnectionSettings();
    }

    /// <summary>
    /// Перечисление типов подключения.
    /// </summary>
    public enum ConnectionType
    {
        PostgreSQL,
        MSSQL
    }
}