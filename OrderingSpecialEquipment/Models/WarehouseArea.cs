using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель территории/зоны склада
    /// </summary>
    [Table("WarehouseAreas")]
    public class WarehouseArea
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// Уникальный идентификатор территории (формат WA000001)
        /// Генерируется базой данных автоматически
        /// </summary>
        [Column("Id")]
        [StringLength(10)]
        [Required]
        [Display(Name = "ID территории", Description = "Уникальный идентификатор территории в формате WA000001")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Наименование территории/зоны
        /// </summary>
        [Column("Name")]
        [StringLength(100)]
        [Required]
        [Display(Name = "Наименование", Description = "Полное наименование территории/зоны")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// ID склада, к которому относится территория
        /// </summary>
        [Column("WarehouseId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Склад", Description = "Ссылка на склад, к которому относится территория")]
        public string WarehouseId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к складу
        /// </summary>
        [ForeignKey("WarehouseId")]
        [Display(Name = "Склад", Description = "Детали склада")]
        public Warehouse? Warehouse { get; set; }

        /// <summary>
        /// Тип территории (Разгрузка, Хранение, Открытое хранение и т.д.)
        /// </summary>
        [Column("AreaType")]
        [StringLength(50)]
        [Display(Name = "Тип территории", Description = "Классификация территории по назначению")]
        public string? AreaType { get; set; }

        /// <summary>
        /// Максимальная вместимость/грузоподъемность
        /// </summary>
        [Column("MaxCapacity")]
        [Display(Name = "Макс. вместимость", Description = "Максимальная вместимость территории (количество единиц техники)")]
        public int? MaxCapacity { get; set; }

        /// <summary>
        /// Флаг активности территории
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активна", Description = "Признак активности территории (может использоваться в заявках)")]
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