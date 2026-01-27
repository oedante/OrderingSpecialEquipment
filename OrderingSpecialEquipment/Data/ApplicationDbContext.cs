using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Класс контекста данных Entity Framework для приложения.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        private readonly string _connectionString;
        private readonly ConnectionType _connectionType;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConnectionService connectionService) // Принимаем IConnectionService через DI
        {
            // Получаем строку подключения из сервиса
            _connectionString = connectionService.GetConnectionString();
            _connectionType = connectionService.CurrentConnectionType;
        }

        // DbSet'ы для каждой сущности
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<EquipmentDependency> EquipmentDependencies { get; set; }
        public DbSet<LessorOrganization> LessorOrganizations { get; set; }
        public DbSet<LicensePlate> LicensePlates { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ShiftRequest> ShiftRequests { get; set; }
        public DbSet<TransportProgram> TransportPrograms { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserDepartmentAccess> UserDepartmentAccesses { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<UserWarehouseAccess> UserWarehouseAccesses { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehouseArea> WarehouseAreas { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                switch (_connectionType)
                {
                    case ConnectionType.PostgreSQL:
                        optionsBuilder.UseNpgsql(_connectionString);
                        break;
                    case ConnectionType.MSSQL:
                        optionsBuilder.UseSqlServer(_connectionString);
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported connection type: {_connectionType}");
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка моделей
            // В идеале, большинство настроек уже задано через Data Annotations в моделях.

            // Настройка столбцов Key (первичные ключи типа int)
            // Для PostgreSQL SERIAL: modelBuilder.Entity<SomeEntity>().Property(e => e.Key).UseIdentityByDefaultColumn();
            // Для MSSQL Identity: modelBuilder.Entity<SomeEntity>().Property(e => e.Key).ValueGeneratedOnAdd();
            // EF Core автоматически настраивает Identity для int Key с атрибутом [Key] в зависимости от провайдера.

            // Явно указываем стратегию генерации значений для Key столбцов, если нужно гарантировать поведение.
            // PostgreSQL:
            if (_connectionType == ConnectionType.PostgreSQL)
            {
                modelBuilder.Entity<AuditLog>().Property(e => e.Key).UseIdentityByDefaultColumn(); // Для PostgreSQL SERIAL
                modelBuilder.Entity<Department>().Property(e => e.Key).UseIdentityByDefaultColumn();
                modelBuilder.Entity<Equipment>().Property(e => e.Key).UseIdentityByDefaultColumn();
                modelBuilder.Entity<EquipmentDependency>().Property(e => e.Key).UseIdentityByDefaultColumn();
                modelBuilder.Entity<LessorOrganization>().Property(e => e.Key).UseIdentityByDefaultColumn();
                modelBuilder.Entity<LicensePlate>().Property(e => e.Key).UseIdentityByDefaultColumn();
                modelBuilder.Entity<Role>().Property(e => e.Key).UseIdentityByDefaultColumn();
                modelBuilder.Entity<ShiftRequest>().Property(e => e.Key).UseIdentityByDefaultColumn();
                modelBuilder.Entity<TransportProgram>().Property(e => e.Key).UseIdentityByDefaultColumn();
                modelBuilder.Entity<User>().Property(e => e.Key).UseIdentityByDefaultColumn();
                modelBuilder.Entity<UserDepartmentAccess>().Property(e => e.Key).UseIdentityByDefaultColumn();
                modelBuilder.Entity<UserFavorite>().Property(e => e.Key).UseIdentityByDefaultColumn();
                modelBuilder.Entity<UserWarehouseAccess>().Property(e => e.Key).UseIdentityByDefaultColumn();
                modelBuilder.Entity<Warehouse>().Property(e => e.Key).UseIdentityByDefaultColumn();
                modelBuilder.Entity<WarehouseArea>().Property(e => e.Key).UseIdentityByDefaultColumn();
            }
            // MSSQL Identity настраивается автоматически через .ValueGeneratedOnAdd(), но если явно:
            // else if (_connectionType == ConnectionType.MSSQL)
            // {
            //     modelBuilder.Entity<AuditLog>().Property(e => e.Key).ValueGeneratedOnAdd();
            //     // ... и так далее для всех остальных
            // }
            // В остальных случаях (или если поведение по умолчанию подходит), ничего указывать не нужно.

            base.OnModelCreating(modelBuilder);
        }
    }
}