using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы организаций-арендодателей.
    /// </summary>
    [Table("LessorOrganizations")]
    public class LessorOrganization
    {
        [Key]
        [Column("Id")]
        [StringLength(10)]
        [Display(Name = "ID арендодателя", Description = "Уникальный идентификатор арендодателя")]
        public string Id { get; set; } = string.Empty;

        [Column("Key")]
        [Display(Name = "Ключ", Description = "Суррогатный ключ (SERIAL)")]
        public int Key { get; set; } // SERIAL

        [Column("Name")]
        [StringLength(200)]
        [Display(Name = "Название", Description = "Название организации-арендодателя")]
        public string Name { get; set; } = string.Empty;

        [Column("INN")]
        [StringLength(12)]
        [Display(Name = "ИНН", Description = "Идентификационный номер налогоплательщика")]
        public string? INN { get; set; }

        [Column("ContactPerson")]
        [StringLength(150)]
        [Display(Name = "Контактное лицо", Description = "ФИО контактного лица")]
        public string? ContactPerson { get; set; }

        [Column("Phone")]
        [StringLength(20)]
        [Display(Name = "Телефон", Description = "Контактный телефон")]
        public string? Phone { get; set; }

        [Column("Email")]
        [StringLength(100)]
        [Display(Name = "Email", Description = "Контактный email")]
        public string? Email { get; set; }

        [Column("Address")]
        [StringLength(500)]
        [Display(Name = "Адрес", Description = "Адрес организации")]
        public string? Address { get; set; }

        [Column("IsActive")]
        [Display(Name = "Активна", Description = "Активна ли организация")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства (опционально, для EF)
        public virtual ICollection<LicensePlate> LicensePlates { get; set; } = new List<LicensePlate>();
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; } = new List<ShiftRequest>();
    }
}