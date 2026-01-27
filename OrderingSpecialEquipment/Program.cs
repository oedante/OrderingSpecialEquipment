using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Data.Repositories;
using OrderingSpecialEquipment.Services;
using OrderingSpecialEquipment.ViewModels;
using Serilog;
using System;
using System.Windows;

namespace OrderingSpecialEquipment
{
    /// <summary>
    /// Класс точки входа в приложение.
    /// Настройка DI контейнера происходит здесь.
    /// </summary>
    public static class Program
    {
        // Используем IHost для управления жизненным циклом приложения и зависимостями
        private static IHost? _host;

        [STAThread]
        public static void Main(string[] args)
        {
            // --- Настройка Serilog ---
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs\\app-.txt",
                              rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            var app = new App();
            _host = CreateHostBuilder(args).Build();

            try
            {
                var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
                var mainWindow = new MainWindow(mainViewModel);

                app.Run(mainWindow);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Приложение завершилось с ошибкой.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Создает HostBuilder с настройками DI.
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        /// <returns>IHostBuilder для дальнейшей настройки.</returns>
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog() // Интегрируем Serilog с Microsoft.Extensions.Logging
                .ConfigureServices((hostContext, services) =>
                {
                    // --- Настройка подключения ---
                    var connectionService = new ConnectionService();
                    var connectionString = connectionService.GetConnectionString();
                    var connectionType = connectionService.CurrentConnectionType;

                    // --- Регистрация DbContext ---
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        switch (connectionType)
                        {
                            case ConnectionType.PostgreSQL:
                                // Убедитесь, что установлен пакет Npgsql.EntityFrameworkCore.PostgreSQL
                                options.UseNpgsql(connectionString);
                                break;
                            case ConnectionType.MSSQL:
                                // Убедитесь, что установлен пакет Microsoft.EntityFrameworkCore.SqlServer
                                options.UseSqlServer(connectionString);
                                break;
                            default:
                                throw new InvalidOperationException($"Unsupported connection type: {connectionType}");
                        }
                    });

                    // --- Регистрация репозиториев (Scoped) ---
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

                    // --- Регистрация сервисов (Singleton) ---
                    services.AddSingleton<IMessageService, MessageService>();
                    services.AddSingleton<IThemeService, ThemeService>();
                    services.AddSingleton<IConnectionService, ConnectionService>();
                    services.AddSingleton<IAuthenticationService, AuthenticationService>();
                    // AuthorizationService зависит от RoleRepository, передаваемого через конструктор
                    // Или сделать AuthorizationService Scoped, если он использует DbContext напрямую (что вероятнее)
                    // services.AddSingleton<IAuthorizationService, AuthorizationService>(); // Старый вариант
                    services.AddScoped<IAuthorizationService, AuthorizationService>(); // Новый вариант
                    // services.AddTransient<IAuthorizationService, AuthorizationService>(); // Также возможный вариант

                    services.AddSingleton<IDataValidationService, DataValidationService>();
                    services.AddSingleton<IExcelExportService, ExcelExportService>();

                    // --- Регистрация ViewModels (Transient) ---
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

                    // Регистрируем Serilog ILogger как Singleton
                    // services.AddSingleton<ILogger>(Log.Logger); // Не обязательно, если используем UseSerilog()
                });
    }
}