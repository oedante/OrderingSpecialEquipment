using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы государственных номеров техники.
    /// </summary>
    [Table("LicensePlates")]
    public class LicensePlate
    {
        [Key]
        [Column("Id")]
        [StringLength(10)]
        [Display(Name = "ID номера", Description = "Уникальный идентификатор госномера")]
        public string Id { get; set; } = string.Empty;

        [Column("Key")]
        [Display(Name = "Ключ", Description = "Суррогатный ключ (SERIAL)")]
        public int Key { get; set; } // SERIAL

        [Column("PlateNumber")]
        [StringLength(20)]
        [Display(Name = "Госномер", Description = "Номерной знак техники")]
        public string PlateNumber { get; set; } = string.Empty;

        [Column("EquipmentId")]
        [StringLength(10)]
        [Display(Name = "ID техники", Description = "ID связанной техники")]
        public string EquipmentId { get; set; } = string.Empty;

        [Column("LessorOrganizationId")]
        [StringLength(10)]
        [Display(Name = "ID арендодателя", Description = "ID организации-арендодателя")]
        public string LessorOrganizationId { get; set; } = string.Empty;

        [Column("Brand")]
        [StringLength(100)]
        [Display(Name = "Марка", Description = "Марка техники")]
        public string? Brand { get; set; }

        [Column("Year")]
        [Display(Name = "Год выпуска", Description = "Год выпуска техники")]
        public int? Year { get; set; }

        [Column("Capacity")]
        [StringLength(50)]
        [Display(Name = "Мощность/Емкость", Description = "Характеристика мощности или емкости")]
        public string? Capacity { get; set; }

        [Column("VIN")]
        [StringLength(50)]
        [Display(Name = "VIN", Description = "Идентификационный номер транспортного средства")]
        public string? VIN { get; set; }

        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Активен ли госномер")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства (опционально, для EF)
        public virtual Equipment Equipment { get; set; } = null!;
        public virtual LessorOrganization LessorOrganization { get; set; } = null!;
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; } = new List<ShiftRequest>();
    }
}