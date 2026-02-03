using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель роли пользователя с правами доступа
    /// </summary>
    [Table("Roles")]
    public class Role
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// Уникальный идентификатор роли (формат RL000001)
        /// Генерируется базой данных автоматически
        /// </summary>
        [Column("Id")]
        [StringLength(10)]
        [Required]
        [Display(Name = "ID роли", Description = "Уникальный идентификатор роли в формате RL000001")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Наименование роли
        /// </summary>
        [Column("Name")]
        [StringLength(50)]
        [Required]
        [Display(Name = "Наименование", Description = "Полное наименование роли")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Код роли (уникальный)
        /// </summary>
        [Column("Code")]
        [StringLength(20)]
        [Required]
        [Display(Name = "Код", Description = "Уникальный код роли (например: ADMIN, MANAGER)")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Описание роли
        /// </summary>
        [Column("Description")]
        [StringLength(200)]
        [Display(Name = "Описание", Description = "Описание назначения роли")]
        public string? Description { get; set; }

        // ========== Права доступа к таблицам (0-Запрещено, 1-Чтение, 2-Запись) ==========

        [Column("TAB_AuditLogs")]
        [Display(Name = "Аудит логов", Description = "Права доступа к таблице аудит логов")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_AuditLogs { get; set; } = 0;

        [Column("TAB_Departments")]
        [Display(Name = "Отделы", Description = "Права доступа к таблице отделов")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_Departments { get; set; } = 0;

        [Column("TAB_EquipmentDependencies")]
        [Display(Name = "Зависимости техники", Description = "Права доступа к таблице зависимостей техники")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_EquipmentDependencies { get; set; } = 0;

        [Column("TAB_Equipments")]
        [Display(Name = "Техника", Description = "Права доступа к таблице техники")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_Equipments { get; set; } = 0;

        [Column("TAB_LessorOrganizations")]
        [Display(Name = "Организации-арендодатели", Description = "Права доступа к таблице организаций-арендодателей")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_LessorOrganizations { get; set; } = 0;

        [Column("TAB_LicensePlates")]
        [Display(Name = "Госномера", Description = "Права доступа к таблице госномеров")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_LicensePlates { get; set; } = 0;

        [Column("TAB_Roles")]
        [Display(Name = "Роли", Description = "Права доступа к таблице ролей")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_Roles { get; set; } = 0;

        [Column("TAB_ShiftRequests")]
        [Display(Name = "Заявки", Description = "Права доступа к таблице заявок")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_ShiftRequests { get; set; } = 0;

        [Column("TAB_TransportProgram")]
        [Display(Name = "Транспортная программа", Description = "Права доступа к таблице транспортной программы")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_TransportProgram { get; set; } = 0;

        [Column("TAB_UserDepartmentAccess")]
        [Display(Name = "Доступ к отделам", Description = "Права доступа к таблице доступа пользователей к отделам")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_UserDepartmentAccess { get; set; } = 0;

        [Column("TAB_UserFavorites")]
        [Display(Name = "Избранное", Description = "Права доступа к таблице избранного пользователей")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_UserFavorites { get; set; } = 0;

        [Column("TAB_Users")]
        [Display(Name = "Пользователи", Description = "Права доступа к таблице пользователей")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_Users { get; set; } = 0;

        [Column("TAB_UserWarehouseAccess")]
        [Display(Name = "Доступ к складам", Description = "Права доступа к таблице доступа пользователей к складам")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_UserWarehouseAccess { get; set; } = 0;

        [Column("TAB_WarehouseAreas")]
        [Display(Name = "Территории складов", Description = "Права доступа к таблице территорий складов")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_WarehouseAreas { get; set; } = 0;

        [Column("TAB_Warehouses")]
        [Display(Name = "Склады", Description = "Права доступа к таблице складов")]
        [Range(0, 2)]
        [DefaultValue(0)]
        public short TAB_Warehouses { get; set; } = 0;

        // ========== Специальные права (битовые флаги) ==========

        [Column("SPEC_ExportData")]
        [Display(Name = "Экспорт данных", Description = "Право на экспорт данных в Excel")]
        [DefaultValue(false)]
        public bool SPEC_ExportData { get; set; } = false;

        [Column("SPEC_ViewReports")]
        [Display(Name = "Просмотр отчетов", Description = "Право на просмотр отчетов")]
        [DefaultValue(false)]
        public bool SPEC_ViewReports { get; set; } = false;

        [Column("SPEC_ManageAllDepartments")]
        [Display(Name = "Управление всеми отделами", Description = "Право на управление всеми отделами (игнорировать ограничения)")]
        [DefaultValue(false)]
        public bool SPEC_ManageAllDepartments { get; set; } = false;

        [Column("SPEC_ManageUsers")]
        [Display(Name = "Управление пользователями", Description = "Право на управление пользователями и ролями")]
        [DefaultValue(false)]
        public bool SPEC_ManageUsers { get; set; } = false;

        [Column("SPEC_SystemAdmin")]
        [Display(Name = "Системный администратор", Description = "Полные системные права")]
        [DefaultValue(false)]
        public bool SPEC_SystemAdmin { get; set; } = false;

        /// <summary>
        /// Флаг системной роли (нельзя удалить)
        /// </summary>
        [Column("IsSystem")]
        [Display(Name = "Системная", Description = "Признак системной роли (нельзя удалить)")]
        [DefaultValue(false)]
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// Флаг активности роли
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активна", Description = "Признак активности роли")]
        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}