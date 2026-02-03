using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель госномера техники с привязкой к конкретной единице
    /// </summary>
    [Table("LicensePlates")]
    public class LicensePlate
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// Уникальный идентификатор записи (формат LP000001)
        /// Генерируется базой данных автоматически
        /// </summary>
        [Column("Id")]
        [StringLength(10)]
        [Required]
        [Display(Name = "ID записи", Description = "Уникальный идентификатор записи в формате LP000001")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Государственный номерной знак
        /// </summary>
        [Column("PlateNumber")]
        [StringLength(20)]
        [Required]
        [Display(Name = "Госномер", Description = "Государственный регистрационный номерной знак")]
        public string PlateNumber { get; set; } = string.Empty;

        /// <summary>
        /// ID техники, к которой относится номер
        /// </summary>
        [Column("EquipmentId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Техника", Description = "Ссылка на технику, к которой относится госномер")]
        public string EquipmentId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к технике
        /// </summary>
        [ForeignKey("EquipmentId")]
        [Display(Name = "Техника", Description = "Детали техники")]
        public Equipment? Equipment { get; set; }

        /// <summary>
        /// ID организации-арендодателя
        /// </summary>
        [Column("LessorOrganizationId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Организация", Description = "Ссылка на организацию-арендодателя")]
        public string LessorOrganizationId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к организации
        /// </summary>
        [ForeignKey("LessorOrganizationId")]
        [Display(Name = "Организация", Description = "Детали организации-арендодателя")]
        public LessorOrganization? LessorOrganization { get; set; }

        /// <summary>
        /// Марка/модель техники
        /// </summary>
        [Column("Brand")]
        [StringLength(100)]
        [Display(Name = "Марка", Description = "Марка и модель техники")]
        public string? Brand { get; set; }

        /// <summary>
        /// Год выпуска
        /// </summary>
        [Column("Year")]
        [Display(Name = "Год выпуска", Description = "Год выпуска техники")]
        [Range(1900, 2100, ErrorMessage = "Неверный год выпуска")]
        public int? Year { get; set; }

        /// <summary>
        /// Грузоподъемность/вместимость
        /// </summary>
        [Column("Capacity")]
        [StringLength(50)]
        [Display(Name = "Характеристики", Description = "Грузоподъемность, объем или другие характеристики")]
        public string? Capacity { get; set; }

        /// <summary>
        /// VIN номер
        /// </summary>
        [Column("VIN")]
        [StringLength(50)]
        [Display(Name = "VIN", Description = "Идентификационный номер транспортного средства")]
        public string? VIN { get; set; }

        /// <summary>
        /// Флаг активности записи
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Признак активности записи (номер действителен)")]
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