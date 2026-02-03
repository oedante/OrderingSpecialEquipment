using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель доступа пользователя к складу
    /// </summary>
    [Table("UserWarehouseAccess")]
    public class UserWarehouseAccess
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// Ключ доступа к отделу пользователя
        /// </summary>
        [Column("UserDepartmentAccessKey")]
        [Required]
        [Display(Name = "Доступ к отделу", Description = "Ссылка на запись доступа пользователя к отделу")]
        public int UserDepartmentAccessKey { get; set; }

        /// <summary>
        /// Навигационное свойство к доступу к отделу
        /// </summary>
        [ForeignKey("UserDepartmentAccessKey")]
        [Display(Name = "Доступ к отделу", Description = "Детали доступа к отделу")]
        public UserDepartmentAccess? UserDepartmentAccess { get; set; }

        /// <summary>
        /// ID склада
        /// </summary>
        [Column("WarehouseId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Склад", Description = "Ссылка на склад")]
        public string WarehouseId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к складу
        /// </summary>
        [ForeignKey("WarehouseId")]
        [Display(Name = "Склад", Description = "Детали склада")]
        public Warehouse? Warehouse { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}