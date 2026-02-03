using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель доступа пользователя к отделу
    /// </summary>
    [Table("UserDepartmentAccess")]
    public class UserDepartmentAccess
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// ID пользователя
        /// </summary>
        [Column("UserId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Пользователь", Description = "Ссылка на пользователя")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к пользователю
        /// </summary>
        [ForeignKey("UserId")]
        [Display(Name = "Пользователь", Description = "Детали пользователя")]
        public User? User { get; set; }

        /// <summary>
        /// ID отдела
        /// </summary>
        [Column("DepartmentId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Отдел", Description = "Ссылка на отдел")]
        public string DepartmentId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к отделу
        /// </summary>
        [ForeignKey("DepartmentId")]
        [Display(Name = "Отдел", Description = "Детали отдела")]
        public Department? Department { get; set; }

        /// <summary>
        /// Флаг доступа ко всем складам отдела
        /// </summary>
        [Column("HasAllWarehouses")]
        [Display(Name = "Все склады", Description = "Признак доступа ко всем складам отдела")]
        [DefaultValue(false)]
        public bool HasAllWarehouses { get; set; } = false;

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}