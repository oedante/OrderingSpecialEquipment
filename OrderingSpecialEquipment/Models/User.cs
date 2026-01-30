using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы пользователей.
    /// </summary>
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("Id")]
        [StringLength(10)]
        [Display(Name = "ID пользователя", Description = "Уникальный идентификатор пользователя")]
        public string Id { get; set; } = string.Empty;

        [Column("Key")]
        [Display(Name = "Ключ", Description = "Суррогатный ключ (SERIAL)")]
        public int Key { get; set; } // SERIAL

        [Column("WindowsLogin")]
        [StringLength(100)]
        [Display(Name = "Логин Windows", Description = "Имя входа пользователя Windows")]
        public string WindowsLogin { get; set; } = string.Empty;

        [Column("FullName")]
        [StringLength(150)]
        [Display(Name = "ФИО", Description = "Полное имя пользователя")]
        public string FullName { get; set; } = string.Empty;

        [Column("Email")]
        [StringLength(100)]
        [Display(Name = "Email", Description = "Контактный email")]
        public string? Email { get; set; }

        [Column("Phone")]
        [StringLength(20)]
        [Display(Name = "Телефон", Description = "Контактный телефон")]
        public string? Phone { get; set; }

        [Column("RoleId")]
        [StringLength(10)]
        [Display(Name = "ID роли", Description = "ID назначенной роли пользователя")]
        public string RoleId { get; set; } = string.Empty;

        [Column("DefaultDepartmentId")]
        [StringLength(10)]
        [Display(Name = "ID отдела по умолчанию", Description = "ID отдела, используемого по умолчанию")]
        public string? DefaultDepartmentId { get; set; }

        [Column("HasAllDepartments")]
        [Display(Name = "Доступ ко всем отделам", Description = "Имеет ли пользователь доступ ко всем отделам")]
        public bool HasAllDepartments { get; set; } = false;

        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Активен ли пользователь")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- НАВИГАЦИОННЫЕ СВОЙСТВА ---
        // Связь: Многие к одному (User -> Role)
        /// <summary>
        /// Роль пользователя.
        /// </summary>
        [ForeignKey("RoleId")] // <-- Оставлено, так как RoleId - это FK в User, указывающий на Role.Key
        public virtual Role? Role { get; set; }

        // Связь: Многие к одному (User -> Department)
        /// <summary>
        /// Отдел по умолчанию.
        /// </summary>
        [ForeignKey("DefaultDepartmentId")] // <-- Оставлено, так как DefaultDepartmentId - это FK в User, указывающий на Department.Id
        public virtual Department? DefaultDepartment { get; set; }

        // Связь: Один ко многим (User -> UserDepartmentAccess)
        /// <summary>
        /// Коллекция записей доступа к отделам.
        /// </summary>
        // УБРАНО: [ForeignKey("UserId")] - EF Core сам поймёт связь по названию свойства UserId в UserDepartmentAccess
        public virtual ICollection<UserDepartmentAccess> UserDepartmentAccesses { get; set; } = new List<UserDepartmentAccess>();

        // Связь: Один ко многим (User -> ShiftRequest)
        /// <summary>
        /// Коллекция заявок, созданных пользователем.
        /// </summary>
        // УБРАНО: [ForeignKey("CreatedByUserId")] - EF Core сам поймёт связь по названию свойства CreatedByUserId в ShiftRequest
        public virtual ICollection<ShiftRequest> CreatedShiftRequests { get; set; } = new List<ShiftRequest>();

        // Связь: Один ко многим (User -> UserFavorite)
        /// <summary>
        /// Коллекция избранных записей пользователя.
        /// </summary>
        // УБРАНО: [ForeignKey("UserId")] - EF Core сам поймёт связь по названию свойства UserId в UserFavorite
        public virtual ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();

        // Связь: Один ко многим (User -> AuditLog)
        /// <summary>
        /// Коллекция логов аудита, созданных пользователем.
        /// </summary>
        // УБРАНО: [ForeignKey("ChangedByUserId")] - EF Core сам поймёт связь по названию свойства ChangedByUserId в AuditLog
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}