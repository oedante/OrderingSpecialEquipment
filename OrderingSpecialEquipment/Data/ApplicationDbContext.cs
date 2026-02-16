using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Converters;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Контекст базы данных приложения
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        #region Конструкторы

        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        /// <param name="options">Опции контекста</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #endregion

        #region DbSet для всех таблиц

        /// <summary>
        /// Отделы
        /// </summary>
        public DbSet<Department> Departments { get; set; }

        /// <summary>
        /// Организации-арендодатели
        /// </summary>
        public DbSet<LessorOrganization> LessorOrganizations { get; set; }

        /// <summary>
        /// Техника
        /// </summary>
        public DbSet<Equipment> Equipments { get; set; }

        /// <summary>
        /// Государственные номера
        /// </summary>
        public DbSet<LicensePlate> LicensePlates { get; set; }

        /// <summary>
        /// Зависимости техники
        /// </summary>
        public DbSet<EquipmentDependency> EquipmentDependencies { get; set; }

        /// <summary>
        /// Транспортная программа
        /// </summary>
        public DbSet<TransportProgram> TransportProgram { get; set; }

        /// <summary>
        /// Роли
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Пользователи
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Доступ пользователей к отделам
        /// </summary>
        public DbSet<UserDepartmentAccess> UserDepartmentAccesses { get; set; }

        /// <summary>
        /// Склады
        /// </summary>
        public DbSet<Warehouse> Warehouses { get; set; }

        /// <summary>
        /// Доступ пользователей к складам
        /// </summary>
        public DbSet<UserWarehouseAccess> UserWarehouseAccesses { get; set; }

        /// <summary>
        /// Территории складов
        /// </summary>
        public DbSet<WarehouseArea> WarehouseAreas { get; set; }

        /// <summary>
        /// Заявки
        /// </summary>
        public DbSet<ShiftRequest> ShiftRequests { get; set; }

        /// <summary>
        /// Избранное пользователей
        /// </summary>
        public DbSet<UserFavorite> UserFavorites { get; set; }

        /// <summary>
        /// Настройки пользователей
        /// </summary>
        public DbSet<UserSetting> UserSettings { get; set; }

        /// <summary>
        /// Журнал аудита
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        #endregion

        #region Конфигурация модели

        /// <summary>
        /// Настройка модели при создании
        /// </summary>
        /// <param name="modelBuilder">Построитель модели</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Глобальный конвертер для всех DateTime полей
            var utcConverter = new UtcDateTimeConverter();
            var nullableUtcConverter = new UtcNullableDateTimeConverter();

            // Применяем ко всем свойствам DateTime в модели
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(utcConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableUtcConverter);
                    }
                }
            }

            #region Конфигурация Departments

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasMany(e => e.Users)
                    .WithOne(e => e.DefaultDepartment)
                    .HasForeignKey(e => e.DefaultDepartmentId)
                    .IsRequired(false);
            });

            #endregion

            #region Конфигурация LessorOrganizations

            modelBuilder.Entity<LessorOrganization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Key).IsUnique();
                entity.HasIndex(e => e.INN).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            #endregion

            #region Конфигурация Equipments

            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            #endregion

            #region Конфигурация LicensePlates

            modelBuilder.Entity<LicensePlate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Key).IsUnique();
                entity.HasIndex(e => e.PlateNumber).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Equipment)
                    .WithMany(e => e.LicensePlates)
                    .HasForeignKey(e => e.EquipmentId);

                entity.HasOne(e => e.LessorOrganization)
                    .WithMany(e => e.LicensePlates)
                    .HasForeignKey(e => e.LessorOrganizationId);
            });

            #endregion

            #region Конфигурация EquipmentDependencies

            modelBuilder.Entity<EquipmentDependency>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.HasIndex(e => new { e.MainEquipmentId, e.DependentEquipmentId }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.MainEquipment)
                    .WithMany(e => e.MainEquipmentDependencies)
                    .HasForeignKey(e => e.MainEquipmentId);

                entity.HasOne(e => e.DependentEquipment)
                    .WithMany(e => e.DependentEquipmentDependencies)
                    .HasForeignKey(e => e.DependentEquipmentId);

                entity.ToTable(t => t.HasCheckConstraint("CHK_EquipmentDependencies_Different",
                    "\"MainEquipmentId\" != \"DependentEquipmentId\""));
            });

            #endregion

            #region Конфигурация TransportProgram

            modelBuilder.Entity<TransportProgram>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.HasIndex(e => new { e.DepartmentId, e.Year, e.EquipmentId }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.TotalYearHours)
                    .HasComputedColumnSql("\"JanuaryHours\" + \"FebruaryHours\" + \"MarchHours\" + \"AprilHours\" + \"MayHours\" + \"JuneHours\" + \"JulyHours\" + \"AugustHours\" + \"SeptemberHours\" + \"OctoberHours\" + \"NovemberHours\" + \"DecemberHours\"", stored: true);

                entity.Property(e => e.TotalYearCost)
                    .HasComputedColumnSql("(\"JanuaryHours\" + \"FebruaryHours\" + \"MarchHours\" + \"AprilHours\" + \"MayHours\" + \"JuneHours\" + \"JulyHours\" + \"AugustHours\" + \"SeptemberHours\" + \"OctoberHours\" + \"NovemberHours\" + \"DecemberHours\") * \"HourlyCost\"", stored: true);

                entity.HasOne(e => e.Department)
                    .WithMany(e => e.TransportPrograms)
                    .HasForeignKey(e => e.DepartmentId);

                entity.HasOne(e => e.Equipment)
                    .WithMany(e => e.TransportPrograms)
                    .HasForeignKey(e => e.EquipmentId);

                entity.ToTable(t => t.HasCheckConstraint("CHK_TransportProgram_Year", "\"Year\" >= 2020 AND \"Year\" <= 2100"));
            });

            #endregion

            #region Конфигурация Roles

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Key).IsUnique();
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            #endregion

            #region Конфигурация Users

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Key).IsUnique();
                entity.HasIndex(e => e.WindowsLogin).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Role)
                    .WithMany(e => e.Users)
                    .HasForeignKey(e => e.RoleId)
                    .HasPrincipalKey(e => e.Id);

                entity.HasOne(e => e.DefaultDepartment)
                    .WithMany(e => e.Users)
                    .HasForeignKey(e => e.DefaultDepartmentId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
            });

            #endregion

            #region Конфигурация UserDepartmentAccess

            modelBuilder.Entity<UserDepartmentAccess>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.HasIndex(e => new { e.UserId, e.DepartmentId }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.User)
                    .WithMany(e => e.UserDepartmentAccesses)
                    .HasForeignKey(e => e.UserId)
                    .HasPrincipalKey(e => e.Id);

                entity.HasOne(e => e.Department)
                    .WithMany(e => e.UserDepartmentAccesses)
                    .HasForeignKey(e => e.DepartmentId)
                    .HasPrincipalKey(e => e.Id);
            });

            #endregion

            #region Конфигурация Warehouses

            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Department)
                    .WithMany(e => e.Warehouses)
                    .HasForeignKey(e => e.DepartmentId)
                    .HasPrincipalKey(e => e.Id);
            });

            #endregion

            #region Конфигурация UserWarehouseAccess

            modelBuilder.Entity<UserWarehouseAccess>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.HasIndex(e => new { e.UserDepartmentAccessKey, e.WarehouseId }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.UserDepartmentAccess)
                    .WithMany(e => e.UserWarehouseAccesses)
                    .HasForeignKey(e => e.UserDepartmentAccessKey)
                    .HasPrincipalKey(e => e.Key)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Warehouse)
                    .WithMany(e => e.UserWarehouseAccesses)
                    .HasForeignKey(e => e.WarehouseId)
                    .HasPrincipalKey(e => e.Id);
            });

            #endregion

            #region Конфигурация WarehouseAreas

            modelBuilder.Entity<WarehouseArea>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Warehouse)
                    .WithMany(e => e.WarehouseAreas)
                    .HasForeignKey(e => e.WarehouseId)
                    .HasPrincipalKey(e => e.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region Конфигурация ShiftRequests

            modelBuilder.Entity<ShiftRequest>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Equipment)
                    .WithMany(e => e.ShiftRequests)
                    .HasForeignKey(e => e.EquipmentId)
                    .HasPrincipalKey(e => e.Id);

                entity.HasOne(e => e.LicensePlate)
                    .WithMany(e => e.ShiftRequests)
                    .HasForeignKey(e => e.LicensePlateId)
                    .HasPrincipalKey(e => e.Id);

                entity.HasOne(e => e.Warehouse)
                    .WithMany(e => e.ShiftRequests)
                    .HasForeignKey(e => e.WarehouseId)
                    .HasPrincipalKey(e => e.Id);

                entity.HasOne(e => e.Area)
                    .WithMany(e => e.ShiftRequests)
                    .HasForeignKey(e => e.AreaId)
                    .HasPrincipalKey(e => e.Id);

                entity.HasOne(e => e.LessorOrganization)
                    .WithMany(e => e.ShiftRequests)
                    .HasForeignKey(e => e.LessorOrganizationId)
                    .HasPrincipalKey(e => e.Id);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(e => e.CreatedShiftRequests)
                    .HasForeignKey(e => e.CreatedByUserId)
                    .HasPrincipalKey(e => e.Id);

                entity.HasOne(e => e.LockedByUser)
                    .WithMany(e => e.LockedShiftRequests)
                    .HasForeignKey(e => e.LockedByUserId)
                    .HasPrincipalKey(e => e.Id);

                entity.HasOne(e => e.Department)
                    .WithMany(e => e.ShiftRequests)
                    .HasForeignKey(e => e.DepartmentId)
                    .HasPrincipalKey(e => e.Id);

                entity.ToTable(t => t.HasCheckConstraint("CHK_ShiftRequests_CancellationHours",
                    "(NOT (\"IsNotProvided\" = true OR \"IsWeatherCancellation\" = true)) OR (\"WorkedHours\" = 0 OR \"WorkedHours\" IS NULL)"));

                entity.ToTable(t => t.HasCheckConstraint("CHK_ShiftRequests_Shift", "\"Shift\" IN (0, 1)"));
            });

            #endregion

            #region Конфигурация UserFavorites

            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.HasIndex(e => new { e.UserId, e.EquipmentId }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.User)
                    .WithMany(e => e.UserFavorites)
                    .HasForeignKey(e => e.UserId)
                    .HasPrincipalKey(e => e.Id);

                entity.HasOne(e => e.Equipment)
                    .WithMany(e => e.UserFavorites)
                    .HasForeignKey(e => e.EquipmentId)
                    .HasPrincipalKey(e => e.Id);
            });

            #endregion

            #region Конфигурация UserSettings

            modelBuilder.Entity<UserSetting>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.HasIndex(e => new { e.UserId, e.SettingKey }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.User)
                    .WithMany(e => e.UserSettings)
                    .HasForeignKey(e => e.UserId)
                    .HasPrincipalKey(e => e.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region Конфигурация AuditLogs

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.ChangedByUser)
                    .WithMany(e => e.AuditLogs)
                    .HasForeignKey(e => e.ChangedByUserId)
                    .HasPrincipalKey(e => e.Id);
            });

            #endregion
        }

        #endregion

        #region Переопределение SaveChanges для автоматического обновления дат

        /// <summary>
        /// Сохранение изменений с автоматическим обновлением дат
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Асинхронное сохранение изменений с автоматическим обновлением дат
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Обновление временных меток
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is UserSetting &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                if (entry.Entity is UserSetting setting)
                {
                    if (entry.State == EntityState.Added)
                    {
                        setting.CreatedAt = DateTime.UtcNow;
                    }
                    setting.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        #endregion
    }
}