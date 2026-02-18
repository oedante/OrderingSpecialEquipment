using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель связи между складом и территорией (многие-ко-многим)
    /// Таблица WarehouseAreaLinks в базе данных
    /// </summary>
    [Table("WarehouseAreaLinks")]
    [Display(Name = "Связь склада и территории", Description = "Связь между складом и территорией")]
    public class WarehouseAreaLink
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
        /// Идентификатор склада
        /// </summary>
        [Required]
        [Column("WarehouseId")]
        [Display(Name = "Склад", Description = "Идентификатор склада")]
        [StringLength(10)]
        public string WarehouseId { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор территории
        /// </summary>
        [Required]
        [Column("AreaId")]
        [Display(Name = "Территория", Description = "Идентификатор территории")]
        [StringLength(10)]
        public string AreaId { get; set; } = string.Empty;

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        #endregion

        #region Навигационные свойства

        /// <summary>
        /// Склад
        /// </summary>
        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; } = null!;

        /// <summary>
        /// Территория
        /// </summary>
        [ForeignKey("AreaId")]
        public virtual WarehouseArea Area { get; set; } = null!;

        #endregion
    }
}