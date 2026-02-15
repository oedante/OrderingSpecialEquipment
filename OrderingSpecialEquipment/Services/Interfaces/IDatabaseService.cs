using OrderingSpecialEquipment.Data;

namespace OrderingSpecialEquipment.Services.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса работы с БД
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// Контекст БД
        /// </summary>
        ApplicationDbContext Context { get; }

        /// <summary>
        /// Подключена ли БД
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Тип БД
        /// </summary>
        DbConnectionFactory.DatabaseType? DatabaseType { get; }

        /// <summary>
        /// Инициализация подключения
        /// </summary>
        Task<bool> InitializeAsync(string connectionString = null);

        /// <summary>
        /// Проверка подключения
        /// </summary>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Закрытие подключения
        /// </summary>
        void CloseConnection();

        /// <summary>
        /// Событие изменения состояния подключения
        /// </summary>
        event EventHandler<bool> ConnectionStateChanged;
    }
}