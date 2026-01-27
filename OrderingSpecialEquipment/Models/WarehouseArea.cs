using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы территорий складов.
    /// </summary>
    [Table("WarehouseAreas")]
    public class WarehouseArea
    {
        [Key]
        [Column("Id")]
        [StringLength(10)]
        [Display(Name = "ID территории", Description = "Уникальный идентификатор территории")]
        public string Id { get; set; } = string.Empty;

        [Column("Key")]
        [Display(Name = "Ключ", Description = "Суррогатный ключ (SERIAL)")]
        public int Key { get; set; } // SERIAL

        [Column("Name")]
        [StringLength(100)]
        [Display(Name = "Название", Description = "Название территории")]
        public string Name { get; set; } = string.Empty;

        [Column("WarehouseId")]
        [StringLength(10)]
        [Display(Name = "ID склада", Description = "ID склада, которому принадлежит территория")]
        public string WarehouseId { get; set; } = string.Empty;

        [Column("AreaType")]
        [StringLength(50)]
        [Display(Name = "Тип территории", Description = "Тип территории (например, Хранение, Разгрузка)")]
        public string? AreaType { get; set; }

        [Column("MaxCapacity")]
        [Display(Name = "Максимальная вместимость", Description = "Максимальная вместимость территории (если применимо)")]
        public int? MaxCapacity { get; set; }

        [Column("IsActive")]
        [Display(Name = "Активна", Description = "Активна ли территория")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства (опционально, для EF)
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; } = new List<ShiftRequest>();
    }
}