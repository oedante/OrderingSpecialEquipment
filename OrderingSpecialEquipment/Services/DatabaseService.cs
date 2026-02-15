using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Services.Interfaces;
using OrderingSpecialEquipment.Utils;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Сервис для работы с подключением к базе данных
    /// </summary>
    public class DatabaseService : IDatabaseService, IDisposable
    {
        #region Поля

        private ApplicationDbContext _context;
        private string _connectionString = string.Empty;
        private DbConnectionFactory.DatabaseType? _databaseType;
        private bool _isConnected;
        private System.Timers.Timer _reconnectionTimer;
        private readonly object _lockObject = new object();

        #endregion

        #region Свойства

        /// <summary>
        /// Контекст БД
        /// </summary>
        public ApplicationDbContext Context
        {
            get
            {
                if (_context == null)
                    throw new InvalidOperationException("База данных не подключена");
                return _context;
            }
        }

        /// <summary>
        /// Подключена ли БД
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// Тип БД
        /// </summary>
        public DbConnectionFactory.DatabaseType? DatabaseType => _databaseType;

        #endregion

        #region События

        /// <summary>
        /// Событие изменения состояния подключения
        /// </summary>
        public event EventHandler<bool> ConnectionStateChanged;

        #endregion

        #region Конструктор и деструктор

        /// <summary>
        /// Конструктор сервиса БД
        /// </summary>
        public DatabaseService()
        {
            // Инициализация таймера для переподключения
            _reconnectionTimer = new System.Timers.Timer(60000); // 60 секунд
            _reconnectionTimer.Elapsed += OnReconnectionTimerElapsed;
            _reconnectionTimer.AutoReset = true;
        }

        /// <summary>
        /// Получение строки подключения
        /// </summary>
        public string GetConnectionString()
        {
            return _connectionString;
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            _reconnectionTimer?.Stop();
            _reconnectionTimer?.Dispose();
            _context?.Dispose();
        }


        #endregion

        #region Публичные методы

        /// <summary>
        /// Инициализация подключения
        /// </summary>
        /// <param name="connectionString">Строка подключения (если null, используется сохраненная)</param>
        public async Task<bool> InitializeAsync(string connectionString = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DatabaseService.InitializeAsync: начало инициализации");

                // Если передана новая строка, сохраняем её
                if (!string.IsNullOrEmpty(connectionString))
                {
                    _connectionString = connectionString;
                    _databaseType = DbConnectionFactory.DetectDatabaseType(connectionString);

                    // Шифруем и сохраняем
                    ConnectionStringHelper.SaveConnectionString(connectionString);

                    System.Diagnostics.Debug.WriteLine($"Используется переданная строка подключения, тип БД: {_databaseType}");
                }
                else
                {
                    // Загружаем сохраненную строку
                    _connectionString = ConnectionStringHelper.LoadConnectionString();
                    if (string.IsNullOrEmpty(_connectionString))
                    {
                        System.Diagnostics.Debug.WriteLine("Строка подключения не найдена");
                        _isConnected = false;
                        ConnectionStateChanged?.Invoke(this, false);
                        return false;
                    }

                    _databaseType = DbConnectionFactory.DetectDatabaseType(_connectionString);
                    System.Diagnostics.Debug.WriteLine($"Загружена сохраненная строка подключения, тип БД: {_databaseType}");
                }

                if (_databaseType == null)
                {
                    System.Diagnostics.Debug.WriteLine("Не удалось определить тип БД");
                    _isConnected = false;
                    ConnectionStateChanged?.Invoke(this, false);
                    return false;
                }

                // Создаем контекст
                var options = DbConnectionFactory.CreateDbContextOptions(_connectionString, _databaseType.Value);

                // Освобождаем старый контекст
                if (_context != null)
                {
                    await _context.DisposeAsync();
                    _context = null;
                }

                _context = new ApplicationDbContext(options);

                // Проверяем подключение
                var result = await TestConnectionAsync();

                if (result)
                {
                    System.Diagnostics.Debug.WriteLine("Подключение к БД успешно установлено");

                    // Запускаем таймер переподключения только если не подключено
                    if (!_isConnected)
                    {
                        _reconnectionTimer.Start();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Не удалось подключиться к БД");
                    _reconnectionTimer.Start();
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации БД: {ex.Message}");
                _isConnected = false;
                _reconnectionTimer.Start();
                ConnectionStateChanged?.Invoke(this, false);
                return false;
            }
        }

        /// <summary>
        /// Проверка подключения
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                if (_context == null)
                {
                    System.Diagnostics.Debug.WriteLine("TestConnectionAsync: контекст не инициализирован");
                    return false;
                }

                var canConnect = await _context.Database.CanConnectAsync();

                System.Diagnostics.Debug.WriteLine($"TestConnectionAsync: результат = {canConnect}");

                if (canConnect != _isConnected)
                {
                    _isConnected = canConnect;
                    ConnectionStateChanged?.Invoke(this, _isConnected);
                }

                return canConnect;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TestConnectionAsync ошибка: {ex.Message}");
                if (_isConnected)
                {
                    _isConnected = false;
                    ConnectionStateChanged?.Invoke(this, false);
                }
                return false;
            }
        }

        /// <summary>
        /// Закрытие подключения
        /// </summary>
        public void CloseConnection()
        {
            _reconnectionTimer.Stop();

            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }

            _isConnected = false;
            ConnectionStateChanged?.Invoke(this, false);

            System.Diagnostics.Debug.WriteLine("Подключение к БД закрыто");
        }

        #endregion

        #region Приватные методы

        /// <summary>
        /// Обработчик таймера переподключения
        /// </summary>
        private async void OnReconnectionTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Предотвращаем параллельные вызовы
            if (!System.Threading.Monitor.TryEnter(_lockObject))
                return;

            try
            {
                if (!_isConnected)
                {
                    System.Diagnostics.Debug.WriteLine("Попытка переподключения к БД...");
                    await InitializeAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при переподключении: {ex.Message}");
                // Игнорируем ошибки при переподключении
            }
            finally
            {
                System.Threading.Monitor.Exit(_lockObject);
            }
        }

        #endregion
    }
}