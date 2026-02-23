using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Services;
using OrderingSpecialEquipment.Services.Interfaces;
using OrderingSpecialEquipment.Utils;
using OrderingSpecialEquipment.ViewModels;
using OrderingSpecialEquipment.Views;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace OrderingSpecialEquipment
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Поля

        private static IServiceProvider? _serviceProvider;
        private ILogger? _logger;

        #endregion

        #region Свойства

        /// <summary>
        /// Сервис провайдер для доступа к DI контейнеру
        /// </summary>
        public static IServiceProvider Services => _serviceProvider ?? throw new InvalidOperationException("Service provider not initialized");

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор приложения
        /// </summary>
        public App()
        {
            // Инициализация логирования
            InitializeLogging();
        }

        #endregion

        #region Обработчики событий приложения

        /// <summary>
        /// Запуск приложения
        /// </summary>
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                _logger?.Information("Приложение запускается");

                // Настройка DI контейнера
                ConfigureServices();

                // ПОЛУЧАЕМ СЕРВИС ТЕМЫ
                var themeService = Services.GetRequiredService<IThemeService>();

                // ЗАГРУЖАЕМ ТЕМУ ДО СОЗДАНИЯ ГЛАВНОГО ОКНА
                themeService.LoadThemeResources();

                // Настройка строки подключения по умолчанию при первом запуске
                await SetupDefaultConnectionStringAsync();

                // Проверка подключения к БД
                bool dbConnected = await CheckDatabaseConnectionAsync();

                if (!dbConnected)
                {
                    MessageBox.Show(
                        "Не удалось подключиться к базе данных. Приложение будет запущено, но некоторые функции будут недоступны.\n\n" +
                        "Для настройки подключения используйте меню 'Файл' -> 'Настройки подключения'.",
                        "Внимание",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                if (dbConnected)
                {
                    await AuthenticateUserAsync();
                }
                else
                {
                    _logger?.Warning("Аутентификация пропущена - нет подключения к БД");
                }

                // СОЗДАЕМ ГЛАВНОЕ ОКНО ПОСЛЕ ЗАГРУЗКИ ТЕМЫ
                var mainWindow = Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Критическая ошибка при запуске приложения");
                MessageBox.Show(
                    $"Произошла критическая ошибка при запуске приложения:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
        }

        /// <summary>
        /// Обработка необработанных исключений
        /// </summary>
        private void Application_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.Error(e.Exception, "Необработанное исключение");

            // Показываем сообщение только для критических ошибок
            if (!e.Exception.Message.Contains("BindingExpression") &&
                !e.Exception.Message.Contains("Set property"))
            {
                MessageBox.Show(
                    $"Произошла ошибка:\n{e.Exception.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Завершение приложения
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            _logger?.Information("Приложение завершает работу");

            // Освобождение ресурсов
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            Log.CloseAndFlush();

            base.OnExit(e);
        }

        #endregion

        #region Методы инициализации

        /// <summary>
        /// Инициализация логирования
        /// </summary>
        private void InitializeLogging()
        {
            try
            {
                string logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "OrderingSpecialEquipment",
                    "logs",
                    "log-.txt");

                // Создаем директорию для логов
                string logDirectory = Path.GetDirectoryName(logPath) ?? string.Empty;
                if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                _logger = Log.Logger;
            }
            catch (Exception ex)
            {
                // Если не удалось инициализировать логирование, используем Debug.WriteLine
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации логирования: {ex.Message}");
            }
        }

        /// <summary>
        /// Настройка DI контейнера
        /// </summary>
        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Регистрация сервисов
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IAuthorizationService, AuthorizationService>();
            services.AddSingleton<IThemeService, ThemeService>();

            // Регистрация фабрики контекстов
            services.AddSingleton<IDbContextFactory, DbContextFactory>();

            // Регистрация DataService
            services.AddScoped(typeof(IDataService<>), typeof(DataService<>));
            services.AddScoped<IShiftRequestService, ShiftRequestService>();
            services.AddScoped<IEquipmentService, EquipmentService>();

            // Регистрация ViewModel
            services.AddTransient<MainWindowViewModel>();

            // Регистрация окон
            services.AddTransient<MainWindow>();
            services.AddTransient<ConnectionSettingsWindow>();
            services.AddTransient<DepartmentsView>();
            services.AddTransient<EquipmentsView>();
            services.AddTransient<WarehousesAndAreasView>();
            services.AddTransient<LessorsAndPlatesView>();
            services.AddTransient<TransportProgramView>();
            services.AddTransient<UsersAndRolesView>();
            services.AddTransient<TransportProgramReportView>();
            services.AddTransient<ShiftRequestsReportView>();
            services.AddTransient<RoleEditDialog>();
            services.AddTransient<UserEditDialog>();

            _serviceProvider = services.BuildServiceProvider();

            _logger?.Information("DI контейнер настроен");
        }

        /// <summary>
        /// Настройка строки подключения по умолчанию при первом запуске
        /// </summary>
        private async Task SetupDefaultConnectionStringAsync()
        {
            try
            {
                // Проверяем, есть ли сохраненная строка подключения
                if (!ConnectionStringHelper.HasConnectionString())
                {
                    _logger?.Information("Строка подключения не найдена, создаем по умолчанию");

                    // Строка подключения по умолчанию для студента
                    string defaultConnectionString = ConnectionStringHelper.GetDefaultConnectionString();

                    // Сохраняем строку подключения
                    ConnectionStringHelper.SaveConnectionString(defaultConnectionString);

                    _logger?.Information("Строка подключения по умолчанию сохранена");
                }
                else
                {
                    _logger?.Information("Используется существующая строка подключения");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка при настройке строки подключения по умолчанию");
            }
        }

        /// <summary>
        /// Проверка подключения к БД
        /// </summary>
        private async Task<bool> CheckDatabaseConnectionAsync()
        {
            var databaseService = Services.GetRequiredService<IDatabaseService>();

            try
            {
                // Пытаемся подключиться с сохраненной строкой
                bool connected = await databaseService.InitializeAsync();

                if (connected)
                {
                    _logger?.Information("Подключение к БД установлено");
                }
                else
                {
                    _logger?.Warning("Не удалось подключиться к БД при запуске");
                }

                return connected;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка при подключении к БД");
                return false;
            }
        }

        /// <summary>
        /// Аутентификация пользователя
        /// </summary>
        private async Task AuthenticateUserAsync()
        {
            var authService = Services.GetRequiredService<IAuthenticationService>();

            try
            {
                bool authenticated = await authService.AuthenticateAsync();

                if (authenticated)
                {
                    _logger?.Information("Пользователь {User} аутентифицирован",
                        authService.CurrentUser?.WindowsLogin);
                }
                else
                {
                    _logger?.Warning("Аутентификация не удалась");

                    MessageBox.Show(
                        "Не удалось выполнить аутентификацию. Проверьте наличие пользователя в системе.\n\n" +
                        "Для первого входа используйте Windows логин: AdminUser",
                        "Внимание",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка при аутентификации");
            }
        }

        #endregion
    }
}