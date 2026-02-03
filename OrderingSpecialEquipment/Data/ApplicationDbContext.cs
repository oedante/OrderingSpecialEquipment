using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Linq;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Контекст базы данных приложения OrderingSpecialEquipment
    /// Управляет всеми сущностями и связями между ними
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        // ========== Таблицы с ключевым полем "Id" ==========

        /// <summary>
        /// Таблица отделов
        /// </summary>
        public DbSet<Department> Departments { get; set; }

        /// <summary>
        /// Таблица техники и оборудования
        /// </summary>
        public DbSet<Equipment> Equipments { get; set; }

        /// <summary>
        /// Таблица организаций-арендодателей
        /// </summary>
        public DbSet<LessorOrganization> LessorOrganizations { get; set; }

        /// <summary>
        /// Таблица госномеров техники
        /// </summary>
        public DbSet<LicensePlate> LicensePlates { get; set; }

        /// <summary>
        /// Таблица ролей пользователей
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Таблица пользователей
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Таблица складов
        /// </summary>
        public DbSet<Warehouse> Warehouses { get; set; }

        /// <summary>
        /// Таблица территорий складов
        /// </summary>
        public DbSet<WarehouseArea> WarehouseAreas { get; set; }

        // ========== Таблицы с ключевым полем "Key" ==========

        /// <summary>
        /// Таблица зависимостей техники
        /// </summary>
        public DbSet<EquipmentDependency> EquipmentDependencies { get; set; }

        /// <summary>
        /// Таблица заявок на смены
        /// </summary>
        public DbSet<ShiftRequest> ShiftRequests { get; set; }

        /// <summary>
        /// Таблица транспортной программы
        /// </summary>
        public DbSet<TransportProgram> TransportProgram { get; set; }

        /// <summary>
        /// Таблица доступа пользователей к отделам
        /// </summary>
        public DbSet<UserDepartmentAccess> UserDepartmentAccess { get; set; }

        /// <summary>
        /// Таблица доступа пользователей к складам
        /// </summary>
        public DbSet<UserWarehouseAccess> UserWarehouseAccess { get; set; }

        /// <summary>
        /// Таблица избранной техники пользователей
        /// </summary>
        public DbSet<UserFavorite> UserFavorites { get; set; }

        /// <summary>
        /// Таблица логов аудита изменений
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// Конструктор контекста для DI
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Настройка модели базы данных
        /// Здесь явно настраиваются все связи между сущностями
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Устанавливаем схему по умолчанию
            modelBuilder.HasDefaultSchema("public");

            // ========== Настройка связей для таблиц с ключевым полем "Id" ==========

            // Отделы
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id).IsUnique();
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // ID генерируется базой данных
                entity.Property(e => e.Key).ValueGeneratedOnAdd();
            });

            // Техника
            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id).IsUnique();
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // ID генерируется базой данных
                entity.Property(e => e.Key).ValueGeneratedOnAdd();
            });

            // Организации-арендодатели
            modelBuilder.Entity<LessorOrganization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id).IsUnique();
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // ID генерируется базой данных
                entity.Property(e => e.Key).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.INN).IsUnique();
            });

            // Госномера
            modelBuilder.Entity<LicensePlate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id).IsUnique();
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // ID генерируется базой данных
                entity.Property(e => e.Key).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.PlateNumber).IsUnique();

                entity.HasOne(e => e.Equipment)
                    .WithMany()
                    .HasForeignKey(e => e.EquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LessorOrganization)
                    .WithMany()
                    .HasForeignKey(e => e.LessorOrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Роли
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id).IsUnique();
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // ID генерируется базой данных
                entity.Property(e => e.Key).ValueGeneratedOnAdd();
            });

            // Пользователи
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id).IsUnique();
                entity.HasIndex(e => e.Key).IsUnique();
                entity.HasIndex(e => e.WindowsLogin).IsUnique();
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // ID генерируется базой данных
                entity.Property(e => e.Key).ValueGeneratedOnAdd();

                entity.HasOne(e => e.Role)
                    .WithMany()
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DefaultDepartment)
                    .WithMany()
                    .HasForeignKey(e => e.DefaultDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Склады
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id).IsUnique();
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // ID генерируется базой данных
                entity.Property(e => e.Key).ValueGeneratedOnAdd();

                entity.HasOne(e => e.Department)
                    .WithMany()
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Территории складов
            modelBuilder.Entity<WarehouseArea>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id).IsUnique();
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // ID генерируется базой данных
                entity.Property(e => e.Key).ValueGeneratedOnAdd();

                entity.HasOne(e => e.Warehouse)
                    .WithMany()
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== Настройка связей для таблиц с ключевым полем "Key" ==========

            // Зависимости техники
            modelBuilder.Entity<EquipmentDependency>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Key).ValueGeneratedOnAdd();

                entity.HasIndex(e => new { e.MainEquipmentId, e.DependentEquipmentId }).IsUnique();

                entity.HasOne(e => e.MainEquipment)
                    .WithMany()
                    .HasForeignKey(e => e.MainEquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DependentEquipment)
                    .WithMany()
                    .HasForeignKey(e => e.DependentEquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasCheckConstraint("CHK_EquipmentDependencies_Different",
                    "\"MainEquipmentId\" != \"DependentEquipmentId\"");
            });

            // Заявки на смены
            modelBuilder.Entity<ShiftRequest>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Key).ValueGeneratedOnAdd();

                entity.HasIndex(e => new { e.Date, e.Shift });
                entity.HasIndex(e => e.EquipmentId);
                entity.HasIndex(e => e.WarehouseId);
                entity.HasIndex(e => e.DepartmentId);
                entity.HasIndex(e => e.CreatedByUserId);
                entity.HasIndex(e => e.LicensePlateId);
                entity.HasIndex(e => new { e.ProgramYear, e.ProgramMonth });

                entity.HasOne(e => e.Equipment)
                    .WithMany()
                    .HasForeignKey(e => e.EquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LicensePlate)
                    .WithMany()
                    .HasForeignKey(e => e.LicensePlateId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Warehouse)
                    .WithMany()
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Area)
                    .WithMany()
                    .HasForeignKey(e => e.AreaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Department)
                    .WithMany()
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LessorOrganization)
                    .WithMany()
                    .HasForeignKey(e => e.LessorOrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasCheckConstraint("CHK_ShiftRequests_ProgramMonth",
                    "\"ProgramMonth\" IS NULL OR (\"ProgramMonth\" >= 1 AND \"ProgramMonth\" <= 12)");
            });

            // Транспортная программа
            modelBuilder.Entity<TransportProgram>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Key).ValueGeneratedOnAdd();

                entity.HasIndex(e => new { e.DepartmentId, e.Year });
                entity.HasIndex(e => e.EquipmentId);

                entity.HasIndex(e => new { e.DepartmentId, e.Year, e.EquipmentId }).IsUnique();

                entity.HasOne(e => e.Department)
                    .WithMany()
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Equipment)
                    .WithMany()
                    .HasForeignKey(e => e.EquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasCheckConstraint("CHK_TransportProgram_Year",
                    "\"Year\" >= 2020 AND \"Year\" <= 2100");

                entity.HasCheckConstraint("CHK_TransportProgram_Hours",
                    "\"JanuaryHours\" >= 0 AND \"FebruaryHours\" >= 0 AND \"MarchHours\" >= 0 AND " +
                    "\"AprilHours\" >= 0 AND \"MayHours\" >= 0 AND \"JuneHours\" >= 0 AND " +
                    "\"JulyHours\" >= 0 AND \"AugustHours\" >= 0 AND \"SeptemberHours\" >= 0 AND " +
                    "\"OctoberHours\" >= 0 AND \"NovemberHours\" >= 0 AND \"DecemberHours\" >= 0");
            });

            // Доступ пользователей к отделам
            modelBuilder.Entity<UserDepartmentAccess>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Key).ValueGeneratedOnAdd();

                entity.HasIndex(e => new { e.UserId, e.DepartmentId }).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.DepartmentId);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Department)
                    .WithMany()
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Доступ пользователей к складам
            modelBuilder.Entity<UserWarehouseAccess>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Key).ValueGeneratedOnAdd();

                entity.HasIndex(e => new { e.UserDepartmentAccessKey, e.WarehouseId }).IsUnique();
                entity.HasIndex(e => e.UserDepartmentAccessKey);
                entity.HasIndex(e => e.WarehouseId);

                entity.HasOne(e => e.UserDepartmentAccess)
                    .WithMany()
                    .HasForeignKey(e => e.UserDepartmentAccessKey)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Warehouse)
                    .WithMany()
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Избранное пользователей
            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Key).ValueGeneratedOnAdd();

                entity.HasIndex(e => new { e.UserId, e.EquipmentId }).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.EquipmentId);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Equipment)
                    .WithMany()
                    .HasForeignKey(e => e.EquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Логи аудита
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Key).ValueGeneratedOnAdd();

                entity.HasIndex(e => new { e.TableName, e.RecordId });
                entity.HasIndex(e => e.ChangedByUserId);
                entity.HasIndex(e => e.ChangedAt);

                entity.HasOne(e => e.ChangedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ChangedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка для работы с временными зонами в UTC
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("timestamp with time zone");
                    }
                }
            }
        }
    }
}