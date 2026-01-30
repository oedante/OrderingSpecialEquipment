using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Data.Repositories;
using OrderingSpecialEquipment.Services;
using OrderingSpecialEquipment.ViewModels;
using OrderingSpecialEquipment.Views;
using Serilog; // Добавляем Serilog
using System;
using System.Windows; // Для MessageBox и Application

namespace OrderingSpecialEquipment
{
    /// <summary>
    /// Класс точки входа в приложение WPF.
    /// Использует IHost для управления зависимостями (DI) и жизненным циклом.
    /// </summary>
    public static class Program
    {
        // Хранит экземпляр IHost для управления приложением и зависимостями.
        private static IHost? _host;

        [STAThread] // Требуется для WPF
        public static void Main(string[] args)
        {
            // --- Настройка Serilog ---
            // Инициализируем глобальный логгер до запуска приложения.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // Минимальный уровень логирования
                .WriteTo.File("logs\\app-.txt", rollingInterval: RollingInterval.Day) // Лог в файл с ротацией по дням
                .WriteTo.Console() // Лог в консоль
                .CreateLogger();

            try
            {

                // --- ПОКАЗЫВАЕМ SPLASH SCREEN НА 3 СЕКУНДЫ ---
                var splash = new Views.SplashScreen();
                splash.Show();

                // Блокируем поток на 3 секунды (это плохо для UX, но просто для демонстрации)
                Thread.Sleep(3000);

                splash.Close(); // Закрываем сплэш

                // Создаём и запускаем IHost, который управляет DI контейнером.
                _host = CreateHostBuilder(args).Build();

                // Создаём экземпляр главного окна WPF
                var app = new App();

                // Получаем MainViewModel из DI контейнера.
                // Это вызовет создание MainViewModel и всех её зависимостей (если они_transient или_singleton зарегистрированы).
                var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();

                // Создаём MainWindow, передав ему MainViewModel.
                // MainViewModel будет установлен как DataContext для окна.
                var mainWindow = new MainWindow(mainViewModel);

                // Запускаем цикл обработки сообщений WPF.
                app.Run(mainWindow);
            }
            catch (Exception ex)
            {
                // Если ошибка происходит до запуска app.Run, она не будет перехвачена в app.DispatcherUnhandledException.
                // Логируем фатальную ошибку и показываем сообщение пользователю.
                Log.Fatal(ex, "Приложение завершилось с ошибкой.");
                MessageBox.Show($"Приложение завершилось с ошибкой: {ex.Message}\n\nСм. файл лога 'logs\\app-.txt' для подробностей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Закрываем и сбрасываем логгер.
                Log.CloseAndFlush();
                _host?.Dispose(); // Освобождаем ресурсы хоста
            }
        }

        /// <summary>
        /// Создаёт IHostBuilder с предварительной настройкой.
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        /// <returns>IHostBuilder для дальнейшей настройки.</returns>
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args) // Создаёт хост с настройками по умолчанию (appsettings.json, environment variables и т.д.)
                .UseSerilog() // Интегрирует Serilog с Microsoft.Extensions.Logging
                .ConfigureServices((hostContext, services) =>
                {
                    // Получаем IConfiguration из контекста хоста
                    var configuration = hostContext.Configuration;

                    // --- ШАГ 1: Получение строки подключения и типа из IConfiguration ---
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    var connectionTypeStr = configuration["ConnectionType"];
                    var connectionType = ConnectionType.PostgreSQL; // Значение по умолчанию

                    if (!string.IsNullOrEmpty(connectionTypeStr) && !Enum.TryParse<ConnectionType>(connectionTypeStr, out connectionType))
                    {
                        Log.Warning("Тип подключения в appsettings.json недействителен: {ConnectionTypeStr}. Используется значение по умолчанию: PostgreSQL.", connectionTypeStr);
                        // connectionType уже установлен на PostgreSQL по умолчанию
                    }

                    // --- ШАГ 2: Проверка строки подключения ---
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        // Если строка подключения отсутствует или пуста, это критическая ошибка для запуска приложения с БД.
                        // Мы можем либо остановить запуск, либо зарегистрировать "фиктивные" сервисы.
                        // В данном случае, выберем остановку с сообщением.
                        Log.Fatal("Строка подключения 'DefaultConnection' в appsettings.json не найдена или пуста.");
                        Console.WriteLine("Ошибка: Строка подключения 'DefaultConnection' в appsettings.json не найдена или пуста. Приложение будет закрыто.");
                        MessageBox.Show("Строка подключения к базе данных не настроена. Приложение будет закрыто.\n\nСм. файл appsettings.json.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(1); // Завершаем процесс с ошибкой
                    }

                    // --- ШАГ 3: Валидация строки подключения ---
                    var tempConnectionService = new ConnectionService(configuration);
                    if (!tempConnectionService.ValidateConnectionString(connectionString))
                    {
                        Log.Fatal("Строка подключения в appsettings.json недействительна: {ConnectionString}.", connectionString);
                        Console.WriteLine($"Ошибка: Строка подключения в appsettings.json недействительна. Приложение будет закрыто.");
                        MessageBox.Show("Строка подключения к базе данных недействительна. Приложение будет закрыто.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(1);
                    }

                    // --- ШАГ 4: Регистрация IConnectionService ---
                    // Теперь, когда строка подключения проверена, регистрируем реальный ConnectionService
                    services.AddSingleton<IConnectionService>(provider => new ConnectionService(configuration));

                    // --- ШАГ 5: Регистрация ApplicationDbContext ---
                    // Регистрируем DbContext с конкретной строкой подключения и типом
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        switch (connectionType)
                        {
                            case ConnectionType.PostgreSQL:
                                options.UseNpgsql(connectionString);
                                break;
                            case ConnectionType.MSSQL:
                                options.UseSqlServer(connectionString);
                                break;
                            default:
                                // Это условие уже проверено выше, но на всякий случай
                                Log.Warning("Неподдерживаемый тип подключения: {ConnectionType}. Используется PostgreSQL.", connectionType);
                                options.UseNpgsql(connectionString); // Или бросить исключение
                                break;
                        }
                        // Включаем логирование SQL-запросов через Serilog (опционально)
                        // options.LogTo(Log.Logger.Information, Microsoft.Extensions.Logging.LogLevel.Information);
                    });

                    // --- ШАГ 6: Регистрация остальных сервисов и ViewModel ---

                    // --- Репозитории (Scoped - один экземпляр на каждый запрос/сессию) ---
                    services.AddScoped<IDepartmentRepository, DepartmentRepository>();
                    services.AddScoped<IEquipmentRepositoryBase, EquipmentRepository>(); // Или IEquipmentRepository
                    services.AddScoped<ILessorOrganizationRepository, LessorOrganizationRepository>();
                    services.AddScoped<ILicensePlateRepository, LicensePlateRepository>();
                    services.AddScoped<IEquipmentDependencyRepository, EquipmentDependencyRepository>();
                    services.AddScoped<ITransportProgramRepository, TransportProgramRepository>();
                    services.AddScoped<IRoleRepository, RoleRepository>();
                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<IUserDepartmentAccessRepository, UserDepartmentAccessRepository>();
                    services.AddScoped<IUserWarehouseAccessRepository, UserWarehouseAccessRepository>();
                    services.AddScoped<IWarehouseRepository, WarehouseRepository>();
                    services.AddScoped<IWarehouseAreaRepository, WarehouseAreaRepository>();
                    services.AddScoped<IShiftRequestRepository, ShiftRequestRepository>();
                    services.AddScoped<IUserFavoriteRepository, UserFavoriteRepository>();
                    services.AddScoped<IAuditLogRepository, AuditLogRepository>();

                    // --- Сервисы (Singleton - один экземпляр на всё приложение) ---
                    services.AddSingleton<IMessageService, MessageService>();
                    services.AddSingleton<IThemeService, ThemeService>();
                    // IConnectionService уже зарегистрирован выше
                    services.AddSingleton<IAuthenticationService, AuthenticationService>();
                    // AuthorizationService зависит от RoleRepository, передаваемого через конструктор
                    // Или сделать AuthorizationService Scoped, если он использует DbContext напрямую (что вероятнее)
                    // services.AddSingleton<IAuthorizationService, AuthorizationService>(); // Старый вариант
                    services.AddScoped<IAuthorizationService, AuthorizationService>(); // Новый вариант
                    // services.AddTransient<IAuthorizationService, AuthorizationService>(); // Также возможный вариант

                    services.AddSingleton<IDataValidationService, DataValidationService>();
                    services.AddSingleton<IExcelExportService, ExcelExportService>();

                    // --- Регистрация новых сервисов ---
                    services.AddScoped<IShiftRequestValidationService, ShiftRequestValidationService>();
                    services.AddScoped<IShiftRequestService, ShiftRequestService>();
                    services.AddScoped<IFavoriteEquipmentService, FavoriteEquipmentService>();

                    // --- ViewModels (Transient - новый экземпляр при каждом запросе) ---
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<EditDepartmentsViewModel>();
                    services.AddTransient<EditEquipmentViewModel>();
                    services.AddTransient<EditLessorOrganizationsViewModel>();
                    services.AddTransient<EditLicensePlatesViewModel>();
                    services.AddTransient<EditRolesViewModel>();
                    services.AddTransient<EditUsersViewModel>();
                    services.AddTransient<EditWarehousesAndAreasViewModel>();
                    services.AddTransient<EditUserAccessViewModel>();
                    services.AddTransient<ReportsViewModel>();
                    services.AddTransient<SettingsViewModel>();

                    // Регистрируем Serilog ILogger как Singleton (необязательно, если используем UseSerilog())
                    // services.AddSingleton<ILogger>(Log.Logger);
                });
    }
}