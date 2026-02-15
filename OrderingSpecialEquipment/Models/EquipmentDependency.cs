using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель зависимости техники
    /// Таблица EquipmentDependencies в базе данных
    /// </summary>
    [Table("EquipmentDependencies")]
    [Display(Name = "Зависимость техники", Description = "Зависимости между техникой (например, для крана нужны стропальщики)")]
    public class EquipmentDependency
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
        /// Идентификатор основной техники
        /// </summary>
        [Required]
        [Column("MainEquipmentId")]
        [Display(Name = "Основная техника", Description = "Идентификатор основной техники")]
        [StringLength(10)]
        public string MainEquipmentId { get; set; }

        /// <summary>
        /// Идентификатор зависимой техники
        /// </summary>
        [Required]
        [Column("DependentEquipmentId")]
        [Display(Name = "Зависимая техника", Description = "Идентификатор зависимой техники")]
        [StringLength(10)]
        public string DependentEquipmentId { get; set; }

        /// <summary>
        /// Требуемое количество
        /// </summary>
        [Column("RequiredCount")]
        [Display(Name = "Количество", Description = "Требуемое количество зависимой техники")]
        [DefaultValue(1)]
        public int RequiredCount { get; set; } = 1;

        /// <summary>
        /// Обязательная ли зависимость
        /// </summary>
        [Column("IsMandatory")]
        [Display(Name = "Обязательная", Description = "Является ли зависимость обязательной")]
        [DefaultValue(true)]
        public bool IsMandatory { get; set; } = true;

        /// <summary>
        /// Описание
        /// </summary>
        [Column("Description")]
        [Display(Name = "Описание", Description = "Описание зависимости")]
        [StringLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        #endregion

        #region Навигационные свойства

        /// <summary>
        /// Основная техника
        /// </summary>
        [ForeignKey("MainEquipmentId")]
        public virtual Equipment MainEquipment { get; set; }

        /// <summary>
        /// Зависимая техника
        /// </summary>
        [ForeignKey("DependentEquipmentId")]
        public virtual Equipment DependentEquipment { get; set; }

        #endregion

        #region Методы

        /// <summary>
        /// Возвращает строковое представление зависимости
        /// </summary>
        public override string ToString()
        {
            return $"{MainEquipmentId} -> {DependentEquipmentId} ({RequiredCount})";
        }

        #endregion
    }
}