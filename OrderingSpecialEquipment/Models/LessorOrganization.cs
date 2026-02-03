using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель организации-арендодателя техники
    /// </summary>
    [Table("LessorOrganizations")]
    public class LessorOrganization
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// Уникальный идентификатор организации (формат LO000001)
        /// Генерируется базой данных автоматически
        /// </summary>
        [Column("Id")]
        [StringLength(10)]
        [Required]
        [Display(Name = "ID организации", Description = "Уникальный идентификатор организации в формате LO000001")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Наименование организации
        /// </summary>
        [Column("Name")]
        [StringLength(200)]
        [Required]
        [Display(Name = "Наименование", Description = "Полное наименование организации-арендодателя")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// ИНН организации
        /// </summary>
        [Column("INN")]
        [StringLength(12)]
        [Display(Name = "ИНН", Description = "Идентификационный номер налогоплательщика")]
        [RegularExpression(@"^\d{10}(\d{2})?$", ErrorMessage = "Неверный формат ИНН")]
        public string? INN { get; set; }

        /// <summary>
        /// Контактное лицо
        /// </summary>
        [Column("ContactPerson")]
        [StringLength(150)]
        [Display(Name = "Контактное лицо", Description = "ФИО контактного лица организации")]
        public string? ContactPerson { get; set; }

        /// <summary>
        /// Телефон для связи
        /// </summary>
        [Column("Phone")]
        [StringLength(20)]
        [Display(Name = "Телефон", Description = "Контактный телефон организации")]
        [DataType(DataType.PhoneNumber)]
        public string? Phone { get; set; }

        /// <summary>
        /// Email для связи
        /// </summary>
        [Column("Email")]
        [StringLength(100)]
        [Display(Name = "Email", Description = "Электронная почта организации")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        /// <summary>
        /// Адрес организации
        /// </summary>
        [Column("Address")]
        [StringLength(500)]
        [Display(Name = "Адрес", Description = "Юридический или фактический адрес организации")]
        public string? Address { get; set; }

        /// <summary>
        /// Флаг активности организации
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активна", Description = "Признак активности организации (может предоставлять технику)")]
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