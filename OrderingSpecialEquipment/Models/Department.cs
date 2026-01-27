using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы отделов.
    /// </summary>
    [Table("Departments")]
    public class Department
    {
        [Key]
        [Column("Id")]
        [StringLength(10)]
        [Display(Name = "ID отдела", Description = "Уникальный идентификатор отдела")]
        public string Id { get; set; } = string.Empty;

        [Column("Key")]
        [Display(Name = "Ключ", Description = "Суррогатный ключ (SERIAL)")]
        public int Key { get; set; } // SERIAL

        [Column("Name")]
        [StringLength(100)]
        [Display(Name = "Название", Description = "Название отдела")]
        public string Name { get; set; } = string.Empty;

        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Активен ли отдел")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства (опционально, для EF)
        public virtual ICollection<UserDepartmentAccess> UserDepartmentAccesses { get; set; } = new List<UserDepartmentAccess>();
        public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; } = new List<ShiftRequest>();
        public virtual ICollection<TransportProgram> TransportPrograms { get; set; } = new List<TransportProgram>();
    }
}