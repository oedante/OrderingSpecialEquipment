using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель пользователя
    /// Таблица Users в базе данных
    /// </summary>
    [Table("Users")]
    [Display(Name = "Пользователь", Description = "Пользователь системы")]
    public class User
    {
        #region Свойства

        /// <summary>
        /// Уникальный идентификатор пользователя (первичный ключ)
        /// Формат: US000001
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Display(Name = "Идентификатор", Description = "Уникальный идентификатор пользователя")]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Числовой ключ (автоинкремент)
        /// </summary>
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Числовой автоинкрементный ключ")]
        public int Key { get; set; }

        /// <summary>
        /// Windows логин пользователя (без домена)
        /// </summary>
        [Required]
        [Column("WindowsLogin")]
        [Display(Name = "Windows логин", Description = "Логин Windows без домена")]
        [StringLength(100)]
        public string WindowsLogin { get; set; } = string.Empty;

        /// <summary>
        /// Полное имя пользователя
        /// </summary>
        [Required]
        [Column("FullName")]
        [Display(Name = "Полное имя", Description = "Полное имя пользователя")]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Email пользователя
        /// </summary>
        [Column("Email")]
        [Display(Name = "Email", Description = "Адрес электронной почты")]
        [DataType(DataType.EmailAddress)]
        [StringLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Телефон пользователя
        /// </summary>
        [Column("Phone")]
        [Display(Name = "Телефон", Description = "Контактный телефон")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Идентификатор роли
        /// </summary>
        [Required]
        [Column("RoleId")]
        [Display(Name = "Роль", Description = "Идентификатор роли пользователя")]
        [StringLength(10)]
        public string RoleId { get; set; } = string.Empty;

        /// <summary>
        /// Отдел по умолчанию
        /// </summary>
        [Column("DefaultDepartmentId")]
        [Display(Name = "Отдел по умолчанию", Description = "Отдел, который используется по умолчанию")]
        [StringLength(10)]
        public string? DefaultDepartmentId { get; set; }

        /// <summary>
        /// Имеет ли доступ ко всем отделам
        /// </summary>
        [Column("HasAllDepartments")]
        [Display(Name = "Все отделы", Description = "Имеет ли пользователь доступ ко всем отделам")]
        [DefaultValue(false)]
        public bool HasAllDepartments { get; set; }

        /// <summary>
        /// Активен ли пользователь
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Признак активности пользователя")]
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
        /// Роль пользователя
        /// </summary>
        [ForeignKey("RoleId")]
        [InverseProperty("Users")]
        public virtual Role? Role { get; set; }

        /// <summary>
        /// Отдел по умолчанию
        /// </summary>
        [ForeignKey("DefaultDepartmentId")]
        [InverseProperty("Users")]
        public virtual Department? DefaultDepartment { get; set; }

        /// <summary>
        /// Список доступов к отделам
        /// </summary>
        [InverseProperty("User")]
        public virtual ICollection<UserDepartmentAccess> UserDepartmentAccesses { get; set; }

        /// <summary>
        /// Список заявок, созданных пользователем
        /// </summary>
        [InverseProperty("CreatedByUser")]
        public virtual ICollection<ShiftRequest> CreatedShiftRequests { get; set; }

        /// <summary>
        /// Список заявок, заблокированных пользователем
        /// </summary>
        [InverseProperty("LockedByUser")]
        public virtual ICollection<ShiftRequest> LockedShiftRequests { get; set; }

        /// <summary>
        /// Список избранного пользователя
        /// </summary>
        [InverseProperty("User")]
        public virtual ICollection<UserFavorite> UserFavorites { get; set; }

        /// <summary>
        /// Список настроек пользователя
        /// </summary>
        [InverseProperty("User")]
        public virtual ICollection<UserSetting> UserSettings { get; set; }

        /// <summary>
        /// Список записей аудита, созданных пользователем
        /// </summary>
        [InverseProperty("ChangedByUser")]
        public virtual ICollection<AuditLog> AuditLogs { get; set; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public User()
        {
            UserDepartmentAccesses = new HashSet<UserDepartmentAccess>();
            CreatedShiftRequests = new HashSet<ShiftRequest>();
            LockedShiftRequests = new HashSet<ShiftRequest>();
            UserFavorites = new HashSet<UserFavorite>();
            UserSettings = new HashSet<UserSetting>();
            AuditLogs = new HashSet<AuditLog>();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Возвращает строковое представление пользователя
        /// </summary>
        public override string ToString()
        {
            return $"{FullName} ({WindowsLogin})";
        }

        #endregion
    }
}