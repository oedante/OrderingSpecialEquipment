using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель роли пользователя
    /// Таблица Roles в базе данных
    /// </summary>
    [Table("Roles")]
    [Display(Name = "Роль", Description = "Роль пользователя в системе")]
    public class Role
    {
        #region Свойства

        /// <summary>
        /// Уникальный идентификатор роли (первичный ключ)
        /// Формат: RL000001
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Display(Name = "Идентификатор", Description = "Уникальный идентификатор роли")]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Числовой ключ (автоинкремент)
        /// </summary>
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Числовой автоинкрементный ключ")]
        public int Key { get; set; }

        /// <summary>
        /// Наименование роли
        /// </summary>
        [Required]
        [Column("Name")]
        [Display(Name = "Наименование", Description = "Наименование роли")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Код роли
        /// </summary>
        [Required]
        [Column("Code")]
        [Display(Name = "Код", Description = "Уникальный код роли")]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Описание роли
        /// </summary>
        [Column("Description")]
        [Display(Name = "Описание", Description = "Описание роли")]
        [StringLength(200)]
        public string? Description { get; set; }

        // Права доступа к таблицам (0-нет, 1-чтение, 2-запись)

        /// <summary>
        /// Доступ к таблице AuditLogs
        /// </summary>
        [Column("TAB_AuditLogs")]
        [Display(Name = "AuditLogs", Description = "Права доступа к журналу аудита")]
        [Range(0, 2)]
        public short TAB_AuditLogs { get; set; }

        /// <summary>
        /// Доступ к таблице Departments
        /// </summary>
        [Column("TAB_Departments")]
        [Display(Name = "Departments", Description = "Права доступа к справочнику отделов")]
        [Range(0, 2)]
        public short TAB_Departments { get; set; }

        /// <summary>
        /// Доступ к таблице EquipmentDependencies
        /// </summary>
        [Column("TAB_EquipmentDependencies")]
        [Display(Name = "EquipmentDependencies", Description = "Права доступа к зависимостям техники")]
        [Range(0, 2)]
        public short TAB_EquipmentDependencies { get; set; }

        /// <summary>
        /// Доступ к таблице Equipments
        /// </summary>
        [Column("TAB_Equipments")]
        [Display(Name = "Equipments", Description = "Права доступа к справочнику техники")]
        [Range(0, 2)]
        public short TAB_Equipments { get; set; }

        /// <summary>
        /// Доступ к таблице LessorOrganizations
        /// </summary>
        [Column("TAB_LessorOrganizations")]
        [Display(Name = "LessorOrganizations", Description = "Права доступа к справочнику арендодателей")]
        [Range(0, 2)]
        public short TAB_LessorOrganizations { get; set; }

        /// <summary>
        /// Доступ к таблице LicensePlates
        /// </summary>
        [Column("TAB_LicensePlates")]
        [Display(Name = "LicensePlates", Description = "Права доступа к справочнику госномеров")]
        [Range(0, 2)]
        public short TAB_LicensePlates { get; set; }

        /// <summary>
        /// Доступ к таблице Roles
        /// </summary>
        [Column("TAB_Roles")]
        [Display(Name = "Roles", Description = "Права доступа к справочнику ролей")]
        [Range(0, 2)]
        public short TAB_Roles { get; set; }

        /// <summary>
        /// Доступ к таблице ShiftRequests
        /// </summary>
        [Column("TAB_ShiftRequests")]
        [Display(Name = "ShiftRequests", Description = "Права доступа к заявкам")]
        [Range(0, 2)]
        public short TAB_ShiftRequests { get; set; }

        /// <summary>
        /// Доступ к таблице TransportProgram
        /// </summary>
        [Column("TAB_TransportProgram")]
        [Display(Name = "TransportProgram", Description = "Права доступа к транспортной программе")]
        [Range(0, 2)]
        public short TAB_TransportProgram { get; set; }

        /// <summary>
        /// Доступ к таблице UserDepartmentAccess
        /// </summary>
        [Column("TAB_UserDepartmentAccess")]
        [Display(Name = "UserDepartmentAccess", Description = "Права доступа к доступу пользователей к отделам")]
        [Range(0, 2)]
        public short TAB_UserDepartmentAccess { get; set; }

        /// <summary>
        /// Доступ к таблице UserFavorites
        /// </summary>
        [Column("TAB_UserFavorites")]
        [Display(Name = "UserFavorites", Description = "Права доступа к избранному")]
        [Range(0, 2)]
        public short TAB_UserFavorites { get; set; }

        /// <summary>
        /// Доступ к таблице Users
        /// </summary>
        [Column("TAB_Users")]
        [Display(Name = "Users", Description = "Права доступа к справочнику пользователей")]
        [Range(0, 2)]
        public short TAB_Users { get; set; }

        /// <summary>
        /// Доступ к таблице UserWarehouseAccess
        /// </summary>
        [Column("TAB_UserWarehouseAccess")]
        [Display(Name = "UserWarehouseAccess", Description = "Права доступа к доступу пользователей к складам")]
        [Range(0, 2)]
        public short TAB_UserWarehouseAccess { get; set; }

        /// <summary>
        /// Доступ к таблице WarehouseAreas
        /// </summary>
        [Column("TAB_WarehouseAreas")]
        [Display(Name = "WarehouseAreas", Description = "Права доступа к территориям складов")]
        [Range(0, 2)]
        public short TAB_WarehouseAreas { get; set; }

        /// <summary>
        /// Доступ к таблице Warehouses
        /// </summary>
        [Column("TAB_Warehouses")]
        [Display(Name = "Warehouses", Description = "Права доступа к справочнику складов")]
        [Range(0, 2)]
        public short TAB_Warehouses { get; set; }

        // Специальные права

        /// <summary>
        /// Право экспорта данных
        /// </summary>
        [Column("SPEC_ExportData")]
        [Display(Name = "Экспорт данных", Description = "Право на экспорт данных")]
        [DefaultValue(false)]
        public bool SPEC_ExportData { get; set; }

        /// <summary>
        /// Право просмотра отчетов
        /// </summary>
        [Column("SPEC_ViewReports")]
        [Display(Name = "Просмотр отчетов", Description = "Право на просмотр отчетов")]
        [DefaultValue(false)]
        public bool SPEC_ViewReports { get; set; }

        /// <summary>
        /// Право управления всеми отделами
        /// </summary>
        [Column("SPEC_ManageAllDepartments")]
        [Display(Name = "Управление всеми отделами", Description = "Право на управление всеми отделами")]
        [DefaultValue(false)]
        public bool SPEC_ManageAllDepartments { get; set; }

        /// <summary>
        /// Право управления пользователями
        /// </summary>
        [Column("SPEC_ManageUsers")]
        [Display(Name = "Управление пользователями", Description = "Право на управление пользователями")]
        [DefaultValue(false)]
        public bool SPEC_ManageUsers { get; set; }

        /// <summary>
        /// Право системного администратора
        /// </summary>
        [Column("SPEC_SystemAdmin")]
        [Display(Name = "Системный администратор", Description = "Право системного администратора")]
        [DefaultValue(false)]
        public bool SPEC_SystemAdmin { get; set; }

        /// <summary>
        /// Право настройки подключения к БД
        /// </summary>
        [Column("SPEC_ConfigureConnection")]
        [Display(Name = "Настройка подключения", Description = "Право настройки подключения к БД")]
        [DefaultValue(false)]
        public bool SPEC_ConfigureConnection { get; set; }

        /// <summary>
        /// Системная ли роль
        /// </summary>
        [Column("IsSystem")]
        [Display(Name = "Системная", Description = "Является ли роль системной")]
        [DefaultValue(false)]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Активна ли роль
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

        #endregion

        #region Навигационные свойства

        /// <summary>
        /// Список пользователей с этой ролью
        /// </summary>
        [InverseProperty("Role")]
        public virtual ICollection<User> Users { get; set; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Role()
        {
            Users = new HashSet<User>();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Возвращает строковое представление роли
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({Code})";
        }

        /// <summary>
        /// Проверяет наличие права на таблицу
        /// </summary>
        public bool HasTableAccess(string tableName, short requiredLevel)
        {
            return tableName switch
            {
                "AuditLogs" => TAB_AuditLogs >= requiredLevel,
                "Departments" => TAB_Departments >= requiredLevel,
                "EquipmentDependencies" => TAB_EquipmentDependencies >= requiredLevel,
                "Equipments" => TAB_Equipments >= requiredLevel,
                "LessorOrganizations" => TAB_LessorOrganizations >= requiredLevel,
                "LicensePlates" => TAB_LicensePlates >= requiredLevel,
                "Roles" => TAB_Roles >= requiredLevel,
                "ShiftRequests" => TAB_ShiftRequests >= requiredLevel,
                "TransportProgram" => TAB_TransportProgram >= requiredLevel,
                "UserDepartmentAccess" => TAB_UserDepartmentAccess >= requiredLevel,
                "UserFavorites" => TAB_UserFavorites >= requiredLevel,
                "Users" => TAB_Users >= requiredLevel,
                "UserWarehouseAccess" => TAB_UserWarehouseAccess >= requiredLevel,
                "WarehouseAreas" => TAB_WarehouseAreas >= requiredLevel,
                "Warehouses" => TAB_Warehouses >= requiredLevel,
                _ => false
            };
        }

        #endregion
    }
}