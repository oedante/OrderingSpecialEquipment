using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы складов.
    /// </summary>
    [Table("Warehouses")]
    public class Warehouse
    {
        [Key]
        [Column("Id")]
        [StringLength(10)]
        [Display(Name = "ID склада", Description = "Уникальный идентификатор склада")]
        public string Id { get; set; } = string.Empty;

        [Column("Key")]
        [Display(Name = "Ключ", Description = "Суррогатный ключ (SERIAL)")]
        public int Key { get; set; } // SERIAL

        [Column("Name")]
        [StringLength(100)]
        [Display(Name = "Название", Description = "Название склада")]
        public string Name { get; set; } = string.Empty;

        [Column("DepartmentId")]
        [StringLength(10)]
        [Display(Name = "ID отдела", Description = "ID отдела, которому принадлежит склад")]
        public string DepartmentId { get; set; } = string.Empty;

        [Column("Address")]
        [StringLength(500)]
        [Display(Name = "Адрес", Description = "Физический адрес склада")]
        public string? Address { get; set; }

        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Активен ли склад")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства (опционально, для EF)
        public virtual Department Department { get; set; } = null!;
        public virtual ICollection<WarehouseArea> WarehouseAreas { get; set; } = new List<WarehouseArea>();
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; } = new List<ShiftRequest>();
        public virtual ICollection<UserWarehouseAccess> UserWarehouseAccesses { get; set; } = new List<UserWarehouseAccess>();
    }
}