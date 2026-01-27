using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы ролей пользователей.
    /// </summary>
    [Table("Roles")]
    public class Role
    {
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Уникальный ключ роли")]
        public int Key { get; set; } // SERIAL

        [Column("Id")]
        [StringLength(10)]
        [Display(Name = "ID роли", Description = "Уникальный идентификатор роли")]
        public string Id { get; set; } = string.Empty;

        [Column("Name")]
        [StringLength(50)]
        [Display(Name = "Название", Description = "Название роли")]
        public string Name { get; set; } = string.Empty;

        [Column("Code")]
        [StringLength(20)]
        [Display(Name = "Код", Description = "Уникальный код роли")]
        public string Code { get; set; } = string.Empty;

        [Column("Description")]
        [StringLength(200)]
        [Display(Name = "Описание", Description = "Описание прав и обязанностей роли")]
        public string? Description { get; set; }

        // Права доступа к таблицам (0-Запрещено, 1-Чтение, 2-Запись)
        [Column("TAB_AuditLogs")]
        [Display(Name = "Права на логи", Description = "Права на таблицу AuditLogs (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_AuditLogs { get; set; } = 0;

        [Column("TAB_Departments")]
        [Display(Name = "Права на отделы", Description = "Права на таблицу Departments (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_Departments { get; set; } = 0;

        [Column("TAB_EquipmentDependencies")]
        [Display(Name = "Права на зависимости", Description = "Права на таблицу EquipmentDependencies (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_EquipmentDependencies { get; set; } = 0;

        [Column("TAB_Equipments")]
        [Display(Name = "Права на технику", Description = "Права на таблицу Equipments (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_Equipments { get; set; } = 0;

        [Column("TAB_LessorOrganizations")]
        [Display(Name = "Права на арендодателей", Description = "Права на таблицу LessorOrganizations (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_LessorOrganizations { get; set; } = 0;

        [Column("TAB_LicensePlates")]
        [Display(Name = "Права на госномера", Description = "Права на таблицу LicensePlates (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_LicensePlates { get; set; } = 0;

        [Column("TAB_Roles")]
        [Display(Name = "Права на роли", Description = "Права на таблицу Roles (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_Roles { get; set; } = 0;

        [Column("TAB_ShiftRequests")]
        [Display(Name = "Права на заявки", Description = "Права на таблицу ShiftRequests (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_ShiftRequests { get; set; } = 0;

        [Column("TAB_TransportProgram")]
        [Display(Name = "Права на транспортную программу", Description = "Права на таблицу TransportProgram (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_TransportProgram { get; set; } = 0;

        [Column("TAB_UserDepartmentAccess")]
        [Display(Name = "Права на доступ к отделам", Description = "Права на таблицу UserDepartmentAccess (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_UserDepartmentAccess { get; set; } = 0;

        [Column("TAB_UserFavorites")]
        [Display(Name = "Права на избранное", Description = "Права на таблицу UserFavorites (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_UserFavorites { get; set; } = 0;

        [Column("TAB_Users")]
        [Display(Name = "Права на пользователей", Description = "Права на таблицу Users (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_Users { get; set; } = 0;

        [Column("TAB_UserWarehouseAccess")]
        [Display(Name = "Права на доступ к складам", Description = "Права на таблицу UserWarehouseAccess (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_UserWarehouseAccess { get; set; } = 0;

        [Column("TAB_WarehouseAreas")]
        [Display(Name = "Права на территории складов", Description = "Права на таблицу WarehouseAreas (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_WarehouseAreas { get; set; } = 0;

        [Column("TAB_Warehouses")]
        [Display(Name = "Права на склады", Description = "Права на таблицу Warehouses (0-Запрещено, 1-Чтение, 2-Запись)")]
        public short TAB_Warehouses { get; set; } = 0;

        // Специальные права (битовые флаги)
        [Column("SPEC_ExportData")]
        [Display(Name = "Право на экспорт", Description = "Может ли роль экспортировать данные")]
        public bool SPEC_ExportData { get; set; } = false;

        [Column("SPEC_ViewReports")]
        [Display(Name = "Право на отчеты", Description = "Может ли роль просматривать отчеты")]
        public bool SPEC_ViewReports { get; set; } = false;

        [Column("SPEC_ManageAllDepartments")]
        [Display(Name = "Право на все отделы", Description = "Может ли роль управлять всеми отделами")]
        public bool SPEC_ManageAllDepartments { get; set; } = false;

        [Column("SPEC_ManageUsers")]
        [Display(Name = "Право на управление пользователями", Description = "Может ли роль управлять пользователями")]
        public bool SPEC_ManageUsers { get; set; } = false;

        [Column("SPEC_SystemAdmin")]
        [Display(Name = "Право системного администратора", Description = "Является ли роль системным администратором")]
        public bool SPEC_SystemAdmin { get; set; } = false;

        [Column("IsSystem")]
        [Display(Name = "Системная роль", Description = "Является ли роль системной (не может быть изменена)")]
        public bool IsSystem { get; set; } = false;

        [Column("IsActive")]
        [Display(Name = "Активна", Description = "Активна ли роль")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства (опционально, для EF)
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}