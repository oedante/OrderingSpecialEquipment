using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы зависимостей техники (например, с краном нужно заказать стропальщиков).
    /// </summary>
    [Table("EquipmentDependencies")]
    public class EquipmentDependency
    {
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Уникальный ключ связи")]
        public int Key { get; set; } // SERIAL

        [Column("MainEquipmentId")]
        [StringLength(10)]
        [Display(Name = "ID основной техники", Description = "ID техники, для которой определяется зависимость")]
        public string MainEquipmentId { get; set; } = string.Empty;

        [Column("DependentEquipmentId")]
        [StringLength(10)]
        [Display(Name = "ID зависимой техники", Description = "ID техники, которая требуется при заказе основной")]
        public string DependentEquipmentId { get; set; } = string.Empty;

        [Column("RequiredCount")]
        [Display(Name = "Количество", Description = "Необходимое количество зависимой техники")]
        public int RequiredCount { get; set; } = 1;

        [Column("IsMandatory")]
        [Display(Name = "Обязательна", Description = "Является ли зависимость обязательной")]
        public bool IsMandatory { get; set; } = true;

        [Column("Description")]
        [StringLength(200)]
        [Display(Name = "Описание", Description = "Описание зависимости")]
        public string? Description { get; set; }

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства (опционально, для EF)
        public virtual Equipment MainEquipment { get; set; } = null!;
        public virtual Equipment DependentEquipment { get; set; } = null!;
    }
}