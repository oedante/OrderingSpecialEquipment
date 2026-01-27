using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы доступа пользователей к складам.
    /// </summary>
    [Table("UserWarehouseAccess")]
    public class UserWarehouseAccess
    {
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Уникальный ключ доступа к складу")]
        public int Key { get; set; } // SERIAL

        [Column("UserDepartmentAccessKey")]
        [Display(Name = "Ключ доступа к отделу", Description = "Ключ записи из UserDepartmentAccess")]
        public int UserDepartmentAccessKey { get; set; }

        [Column("WarehouseId")]
        [StringLength(10)]
        [Display(Name = "ID склада", Description = "ID склада, к которому есть доступ")]
        public string WarehouseId { get; set; } = string.Empty;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства (опционально, для EF)
        public virtual UserDepartmentAccess UserDepartmentAccess { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
    }
}