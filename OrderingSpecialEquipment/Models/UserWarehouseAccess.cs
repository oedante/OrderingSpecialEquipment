using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель доступа пользователя к складам
    /// Таблица UserWarehouseAccess в базе данных
    /// </summary>
    [Table("UserWarehouseAccess")]
    [Display(Name = "Доступ к складам", Description = "Настройки доступа пользователя к складам")]
    public class UserWarehouseAccess
    {
        #region Свойства

        /// <summary>
        /// Числовой ключ (первичный ключ)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Числовой автоинкрементный ключ")]
        public int Key { get; set; }

        /// <summary>
        /// Ключ доступа к отделу
        /// </summary>
        [Required]
        [Column("UserDepartmentAccessKey")]
        [Display(Name = "Ключ доступа к отделу", Description = "Ключ записи доступа к отделу")]
        public int UserDepartmentAccessKey { get; set; }

        /// <summary>
        /// Идентификатор склада
        /// </summary>
        [Required]
        [Column("WarehouseId")]
        [Display(Name = "Склад", Description = "Идентификатор склада")]
        [StringLength(10)]
        public string WarehouseId { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        #endregion

        #region Навигационные свойства

        /// <summary>
        /// Доступ к отделу
        /// </summary>
        [ForeignKey("UserDepartmentAccessKey")]
        public virtual UserDepartmentAccess UserDepartmentAccess { get; set; }

        /// <summary>
        /// Склад
        /// </summary>
        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }

        #endregion
    }
}