using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Services;
using OrderingSpecialEquipment.ViewModels;
using OrderingSpecialEquipment.Views;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;

namespace OrderingSpecialEquipment
{
    /// <summary>
    /// Логика взаимодействия для приложения
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;
        private IServiceProvider? _serviceProvider;

        /// <summary>
        /// Обработчик запуска приложения
        /// </summary>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Устанавливаем глобальные настройки для работы с временными зонами
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

            // Создаем хост приложения
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();

            // Получаем сервис провайдер
            _serviceProvider = _host.Services;

            // Запускаем приложение
            _ = StartApplicationAsync();
        }

        /// <summary>
        /// Настройка сервисов
        /// </summary>
        private void ConfigureServices(IServiceCollection services)
        {
            // Добавляем контекст базы данных
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString("DefaultConnection");
                bool isPostgres = connectionString?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true ||
                                 connectionString?.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase) == true;

                if (isPostgres)
                {
                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                        npgsqlOptions.CommandTimeout(120);
                    });
                }
                else
                {
                    options.UseSqlServer(connectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                        sqlServerOptions.CommandTimeout(120);
                    });
                }
            }, ServiceLifetime.Scoped);

            // Регистрируем репозитории
            services.AddScoped<IGenericRepository<Models.Department>, GenericRepository<Models.Department>>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IGenericRepository<Models.Equipment>, GenericRepository<Models.Equipment>>();
            services.AddScoped<IEquipmentRepository, EquipmentRepository>();
            services.AddScoped<IGenericRepository<Models.User>, GenericRepository<Models.User>>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGenericRepository<Models.ShiftRequest>, GenericRepository<Models.ShiftRequest>>();
            services.AddScoped<IShiftRequestRepository, ShiftRequestRepository>();
            services.AddScoped<IGenericRepository<Models.Warehouse>, GenericRepository<Models.Warehouse>>();
            services.AddScoped<IGenericRepository<Models.WarehouseArea>, GenericRepository<Models.WarehouseArea>>();
            services.AddScoped<IGenericRepository<Models.LessorOrganization>, GenericRepository<Models.LessorOrganization>>();
            services.AddScoped<ILessorOrganizationRepository, LessorOrganizationRepository>();
            services.AddScoped<IGenericRepository<Models.LicensePlate>, GenericRepository<Models.LicensePlate>>();
            services.AddScoped<ILicensePlateRepository, LicensePlateRepository>();
            services.AddScoped<IGenericRepository<Models.Role>, GenericRepository<Models.Role>>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IGenericRepository<Models.EquipmentDependency>, GenericRepository<Models.EquipmentDependency>>();
            services.AddScoped<IEquipmentDependencyRepository, EquipmentDependencyRepository>();
            services.AddScoped<IGenericRepository<Models.TransportProgram>, GenericRepository<Models.TransportProgram>>();
            services.AddScoped<ITransportProgramRepository, TransportProgramRepository>();
            services.AddScoped<IGenericRepository<Models.UserDepartmentAccess>, GenericRepository<Models.UserDepartmentAccess>>();
            services.AddScoped<IUserDepartmentAccessRepository, UserDepartmentAccessRepository>();
            services.AddScoped<IGenericRepository<Models.UserWarehouseAccess>, GenericRepository<Models.UserWarehouseAccess>>();
            services.AddScoped<IUserWarehouseAccessRepository, UserWarehouseAccessRepository>();
            services.AddScoped<IGenericRepository<Models.UserFavorite>, GenericRepository<Models.UserFavorite>>();
            services.AddScoped<IUserFavoriteRepository, UserFavoriteRepository>();
            services.AddScoped<IGenericRepository<Models.AuditLog>, GenericRepository<Models.AuditLog>>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            // Регистрируем сервисы
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IFavoriteEquipmentService, FavoriteEquipmentService>();
            services.AddScoped<IShiftRequestService, ShiftRequestService>();
            services.AddScoped<IEquipmentDependencyService, EquipmentDependencyService>();
            services.AddScoped<Services.Reports.IReportService, Services.Reports.ReportService>();

            // Регистрируем ViewModel
            services.AddTransient<MainViewModel>();
            services.AddTransient<EditDepartmentsViewModel>();
            services.AddTransient<EditEquipmentsViewModel>();
            services.AddTransient<EditWarehousesViewModel>();
            services.AddTransient<EditAccessRightsViewModel>();
            services.AddTransient<EditUsersViewModel>();
            services.AddTransient<EditRolesViewModel>();
            services.AddTransient<EditLessorOrganizationsViewModel>();
            services.AddTransient<EditLicensePlatesViewModel>();
            services.AddTransient<EditTransportProgramViewModel>();
            services.AddTransient<EditEquipmentDependenciesViewModel>();
            services.AddTransient<ViewModels.Reports.ReportPreviewViewModel>();
            services.AddTransient<ViewModels.Reports.ReportParametersViewModel>();

            // Регистрируем конвертеры
            services.AddSingleton<Converters.BoolToVisibilityConverter>();
            services.AddSingleton<Converters.BoolToWidthConverter>();
            services.AddSingleton<Converters.BoolToYesNoConverter>();
            services.AddSingleton<Converters.ShiftConverter>();
            services.AddSingleton<Converters.InverseBooleanConverter>();
            services.AddSingleton<Converters.NullToVisibilityConverter>();
            services.AddSingleton<Converters.DateTimeFormatConverter>();
            services.AddSingleton<Converters.NumberFormatConverter>();
            services.AddSingleton<Converters.CurrencyConverter>();
        }

        /// <summary>
        /// Запуск приложения с применением темы
        /// </summary>
        private async Task StartApplicationAsync()
        {
            try
            {
                // Получаем сервис тем
                var themeService = _serviceProvider!.GetRequiredService<IThemeService>();

                // Загружаем предпочтения темы
                string themeName = themeService.LoadThemePreference();

                // Применяем тему
                themeService.ApplyTheme(themeName);

                // Получаем главную ViewModel
                var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();

                // Создаем главное окно
                var mainWindow = new MainWindow
                {
                    DataContext = mainViewModel
                };

                // Показываем окно
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка запуска приложения: {ex.Message}\n\n{ex.StackTrace}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
        }

        /// <summary>
        /// Обработчик завершения приложения
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            // Освобождаем ресурсы хоста
            _host?.Dispose();

            base.OnExit(e);
        }
    }
}