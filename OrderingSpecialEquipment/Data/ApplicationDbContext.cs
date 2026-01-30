using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Класс контекста данных Entity Framework для приложения.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet'ы для каждой сущности
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Equipment> Equipments { get; set; } = null!;
        public DbSet<EquipmentDependency> EquipmentDependencies { get; set; } = null!;
        public DbSet<LessorOrganization> LessorOrganizations { get; set; } = null!;
        public DbSet<LicensePlate> LicensePlates { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<ShiftRequest> ShiftRequests { get; set; } = null!;
        public DbSet<TransportProgram> TransportPrograms { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserDepartmentAccess> UserDepartmentAccesses { get; set; } = null!;
        public DbSet<UserFavorite> UserFavorites { get; set; } = null!;
        public DbSet<UserWarehouseAccess> UserWarehouseAccesses { get; set; } = null!;
        public DbSet<Warehouse> Warehouses { get; set; } = null!;
        public DbSet<WarehouseArea> WarehouseAreas { get; set; } = null!;
        // AppSettings, если используется как сущность, добавьте DbSet<AppSettings> AppSettings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // --- Настройка первичных ключей (SERIAL для PostgreSQL) ---
            // Для сущностей с Key (int)
            modelBuilder.Entity<AuditLog>().Property(e => e.Key).UseSerialColumn(); // PostgreSQL SERIAL
            modelBuilder.Entity<Department>().Property(e => e.Key).UseSerialColumn();
            modelBuilder.Entity<Equipment>().Property(e => e.Key).UseSerialColumn();
            modelBuilder.Entity<EquipmentDependency>().Property(e => e.Key).UseSerialColumn();
            modelBuilder.Entity<LessorOrganization>().Property(e => e.Key).UseSerialColumn();
            modelBuilder.Entity<LicensePlate>().Property(e => e.Key).UseSerialColumn();
            modelBuilder.Entity<Role>().Property(e => e.Key).UseSerialColumn();
            modelBuilder.Entity<ShiftRequest>().Property(e => e.Key).UseSerialColumn();
            modelBuilder.Entity<TransportProgram>().Property(e => e.Key).UseSerialColumn();
            modelBuilder.Entity<User>().Property(e => e.Key).UseSerialColumn();
            modelBuilder.Entity<UserDepartmentAccess>().Property(e => e.Key).UseSerialColumn();
            modelBuilder.Entity<UserFavorite>().Property(e => e.Key).UseSerialColumn();
            modelBuilder.Entity<UserWarehouseAccess>().Property(e => e.Key).UseSerialColumn();
            modelBuilder.Entity<Warehouse>().Property(e => e.Key).UseSerialColumn();
            modelBuilder.Entity<WarehouseArea>().Property(e => e.Key).UseSerialColumn();

            // --- Явная настройка связей (Relationships) ---

            // --- UserFavorite -> User & Equipment ---
            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.Equipment)
                .WithMany(e => e.UserFavorites) // Предполагаем, что в Equipment есть List<UserFavorite>
                .HasForeignKey(uf => uf.EquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- User -> Role ---
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- User -> DefaultDepartment ---
            modelBuilder.Entity<User>()
                .HasOne(u => u.DefaultDepartment)
                .WithMany() // У Department нет навигационного свойства обратно к User (DefaultDepartment)
                .HasForeignKey(u => u.DefaultDepartmentId)
                .OnDelete(DeleteBehavior.SetNull); // Или Restrict, если поле не nullable

            // --- User -> UserDepartmentAccesses ---
            modelBuilder.Entity<User>()
                .HasMany(u => u.UserDepartmentAccesses)
                .WithOne(uda => uda.User)
                .HasForeignKey(uda => uda.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- User -> CreatedShiftRequests ---
            modelBuilder.Entity<User>()
                .HasMany(u => u.CreatedShiftRequests)
                .WithOne(sr => sr.CreatedByUser!)
                .HasForeignKey(sr => sr.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- User -> AuditLogs ---
            modelBuilder.Entity<User>()
                .HasMany(u => u.AuditLogs)
                .WithOne(al => al.ChangedByUser!)
                .HasForeignKey(al => al.ChangedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- UserDepartmentAccess -> User, Department ---
            modelBuilder.Entity<UserDepartmentAccess>()
                .HasOne(uda => uda.User)
                .WithMany(u => u.UserDepartmentAccesses)
                .HasForeignKey(uda => uda.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            modelBuilder.Entity<UserDepartmentAccess>()
                .HasOne(uda => uda.Department)
                .WithMany(d => d.UserDepartmentAccesses) // Предполагаем, что в Department есть List<UserDepartmentAccess>
                .HasForeignKey(uda => uda.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- UserDepartmentAccess -> UserWarehouseAccesses ---
            modelBuilder.Entity<UserDepartmentAccess>()
                .HasMany(uda => uda.UserWarehouseAccesses)
                .WithOne(uwa => uwa.UserDepartmentAccess)
                .HasForeignKey(uwa => uwa.UserDepartmentAccessKey)
                .OnDelete(DeleteBehavior.Cascade); // При удалении UDA, удаляются связанные UWA

            // --- Department -> UserDepartmentAccesses ---
            modelBuilder.Entity<Department>()
                .HasMany(d => d.UserDepartmentAccesses)
                .WithOne(uda => uda.Department)
                .HasForeignKey(uda => uda.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- Department -> Warehouses ---
            modelBuilder.Entity<Department>()
                .HasMany(d => d.Warehouses)
                .WithOne(w => w.Department)
                .HasForeignKey(w => w.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- Warehouse -> Department ---
            modelBuilder.Entity<Warehouse>()
                .HasOne(w => w.Department)
                .WithMany(d => d.Warehouses)
                .HasForeignKey(w => w.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- Warehouse -> WarehouseAreas ---
            modelBuilder.Entity<Warehouse>()
                .HasMany(w => w.WarehouseAreas)
                .WithOne(wa => wa.Warehouse!)
                .HasForeignKey(wa => wa.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении склада, территории удаляются

            // --- WarehouseArea -> Warehouse ---
            modelBuilder.Entity<WarehouseArea>()
                .HasOne(wa => wa.Warehouse)
                .WithMany(w => w.WarehouseAreas)
                .HasForeignKey(wa => wa.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- EquipmentDependency -> MainEquipment & DependentEquipment ---
            modelBuilder.Entity<EquipmentDependency>()
                .HasOne(ed => ed.MainEquipment)
                .WithMany(e => e.EquipmentDependenciesAsMain) // Предполагаем, что в Equipment есть List<EquipmentDependency>
                .HasForeignKey(ed => ed.MainEquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            modelBuilder.Entity<EquipmentDependency>()
                .HasOne(ed => ed.DependentEquipment)
                .WithMany(e => e.EquipmentDependenciesAsDependent) // Предполагаем, что в Equipment есть List<EquipmentDependency>
                .HasForeignKey(ed => ed.DependentEquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- ShiftRequest -> Equipment ---
            modelBuilder.Entity<ShiftRequest>()
                .HasOne(sr => sr.Equipment)
                .WithMany(e => e.ShiftRequests) // Предполагаем, что в Equipment есть List<ShiftRequest>
                .HasForeignKey(sr => sr.EquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- ShiftRequest -> LicensePlate ---
            modelBuilder.Entity<ShiftRequest>()
                .HasOne(sr => sr.LicensePlate)
                .WithMany(lp => lp.ShiftRequests) // Предполагаем, что в LicensePlate есть List<ShiftRequest>
                .HasForeignKey(sr => sr.LicensePlateId)
                .OnDelete(DeleteBehavior.SetNull); // Или Restrict, если поле не nullable

            // --- ShiftRequest -> Warehouse ---
            modelBuilder.Entity<ShiftRequest>()
                .HasOne(sr => sr.Warehouse)
                .WithMany(w => w.ShiftRequests) // Предполагаем, что в Warehouse есть List<ShiftRequest>
                .HasForeignKey(sr => sr.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- ShiftRequest -> WarehouseArea ---
            modelBuilder.Entity<ShiftRequest>()
                .HasOne(sr => sr.Area)
                .WithMany(wa => wa.ShiftRequests) // Предполагаем, что в WarehouseArea есть List<ShiftRequest>
                .HasForeignKey(sr => sr.AreaId)
                .OnDelete(DeleteBehavior.SetNull); // Или Restrict, если поле не nullable

            // --- ShiftRequest -> Department ---
            modelBuilder.Entity<ShiftRequest>()
                .HasOne(sr => sr.Department)
                .WithMany(d => d.ShiftRequests) // Предполагаем, что в Department есть List<ShiftRequest>
                .HasForeignKey(sr => sr.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull); // Или Restrict, если поле не nullable

            // --- ShiftRequest -> LessorOrganization ---
            modelBuilder.Entity<ShiftRequest>()
                .HasOne(sr => sr.LessorOrganization)
                .WithMany(lo => lo.ShiftRequests) // Предполагаем, что в LessorOrganization есть List<ShiftRequest>
                .HasForeignKey(sr => sr.LessorOrganizationId)
                .OnDelete(DeleteBehavior.SetNull); // Или Restrict, если поле не nullable

            // --- LicensePlate -> Equipment ---
            modelBuilder.Entity<LicensePlate>()
                .HasOne(lp => lp.Equipment)
                .WithMany(e => e.LicensePlates) // Предполагаем, что в Equipment есть List<LicensePlate>
                .HasForeignKey(lp => lp.EquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- LicensePlate -> LessorOrganization ---
            modelBuilder.Entity<LicensePlate>()
                .HasOne(lp => lp.LessorOrganization)
                .WithMany(lo => lo.LicensePlates) // Предполагаем, что в LessorOrganization есть List<LicensePlate>
                .HasForeignKey(lp => lp.LessorOrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- AuditLog -> User (ChangedByUser) ---
            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.ChangedByUser)
                .WithMany(u => u.AuditLogs) // Убедитесь, что в User есть List<AuditLog>
                .HasForeignKey(al => al.ChangedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- TransportProgram -> Department ---
            modelBuilder.Entity<TransportProgram>()
                .HasOne(tp => tp.Department)
                .WithMany(d => d.TransportPrograms) // Предполагаем, что в Department есть List<TransportProgram>
                .HasForeignKey(tp => tp.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            // --- TransportProgram -> Equipment ---
            modelBuilder.Entity<TransportProgram>()
                .HasOne(tp => tp.Equipment)
                .WithMany(e => e.TransportPrograms) // Предполагаем, что в Equipment есть List<TransportProgram>
                .HasForeignKey(tp => tp.EquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Или Restrict

            base.OnModelCreating(modelBuilder);
        }
    }
}