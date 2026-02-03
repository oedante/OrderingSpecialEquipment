using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель пользователя системы
    /// </summary>
    [Table("Users")]
    public class User
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// Уникальный идентификатор пользователя (формат US000001)
        /// Генерируется базой данных автоматически
        /// </summary>
        [Column("Id")]
        [StringLength(10)]
        [Required]
        [Display(Name = "ID пользователя", Description = "Уникальный идентификатор пользователя в формате US000001")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Логин Windows пользователя (в формате DOMAIN\username)
        /// </summary>
        [Column("WindowsLogin")]
        [StringLength(100)]
        [Required]
        [Display(Name = "Windows логин", Description = "Логин пользователя в домене Windows")]
        public string WindowsLogin { get; set; } = string.Empty;

        /// <summary>
        /// Полное имя пользователя
        /// </summary>
        [Column("FullName")]
        [StringLength(150)]
        [Required]
        [Display(Name = "ФИО", Description = "Полное имя пользователя")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Email пользователя
        /// </summary>
        [Column("Email")]
        [StringLength(100)]
        [Display(Name = "Email", Description = "Электронная почта пользователя")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        /// <summary>
        /// Телефон пользователя
        /// </summary>
        [Column("Phone")]
        [StringLength(20)]
        [Display(Name = "Телефон", Description = "Контактный телефон пользователя")]
        [DataType(DataType.PhoneNumber)]
        public string? Phone { get; set; }

        /// <summary>
        /// ID роли пользователя
        /// </summary>
        [Column("RoleId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Роль", Description = "Ссылка на роль пользователя")]
        public string RoleId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к роли
        /// </summary>
        [ForeignKey("RoleId")]
        [Display(Name = "Роль", Description = "Детали роли пользователя")]
        public Role? Role { get; set; }

        /// <summary>
        /// ID отдела по умолчанию
        /// </summary>
        [Column("DefaultDepartmentId")]
        [StringLength(10)]
        [Display(Name = "Отдел по умолчанию", Description = "Отдел, используемый по умолчанию при создании заявок")]
        public string? DefaultDepartmentId { get; set; }

        /// <summary>
        /// Навигационное свойство к отделу по умолчанию
        /// </summary>
        [ForeignKey("DefaultDepartmentId")]
        [Display(Name = "Отдел по умолчанию", Description = "Детали отдела по умолчанию")]
        public Department? DefaultDepartment { get; set; }

        /// <summary>
        /// Флаг доступа ко всем отделам
        /// </summary>
        [Column("HasAllDepartments")]
        [Display(Name = "Все отделы", Description = "Признак доступа ко всем отделам (игнорировать ограничения)")]
        [DefaultValue(false)]
        public bool HasAllDepartments { get; set; } = false;

        /// <summary>
        /// Флаг активности пользователя
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Признак активности пользователя в системе")]
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