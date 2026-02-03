using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель зависимости техники (какая техника требует другую технику)
    /// </summary>
    [Table("EquipmentDependencies")]
    public class EquipmentDependency
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// ID основной техники
        /// </summary>
        [Column("MainEquipmentId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Основная техника", Description = "Ссылка на основную технику")]
        public string MainEquipmentId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к основной технике
        /// </summary>
        [ForeignKey("MainEquipmentId")]
        [Display(Name = "Основная техника", Description = "Детали основной техники")]
        public Equipment? MainEquipment { get; set; }

        /// <summary>
        /// ID зависимой техники
        /// </summary>
        [Column("DependentEquipmentId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Зависимая техника", Description = "Ссылка на зависимую технику")]
        public string DependentEquipmentId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к зависимой технике
        /// </summary>
        [ForeignKey("DependentEquipmentId")]
        [Display(Name = "Зависимая техника", Description = "Детали зависимой техники")]
        public Equipment? DependentEquipment { get; set; }

        /// <summary>
        /// Требуемое количество зависимой техники
        /// </summary>
        [Column("RequiredCount")]
        [Required]
        [Display(Name = "Требуемое количество", Description = "Количество зависимой техники, требуемое для основной")]
        [Range(1, int.MaxValue)]
        [DefaultValue(1)]
        public int RequiredCount { get; set; } = 1;

        /// <summary>
        /// Флаг обязательности зависимости
        /// </summary>
        [Column("IsMandatory")]
        [Display(Name = "Обязательная", Description = "Признак обязательности зависимости")]
        [DefaultValue(true)]
        public bool IsMandatory { get; set; } = true;

        /// <summary>
        /// Описание зависимости
        /// </summary>
        [Column("Description")]
        [StringLength(200)]
        [Display(Name = "Описание", Description = "Описание зависимости между техниками")]
        public string? Description { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}