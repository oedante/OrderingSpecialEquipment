using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель склада организации
    /// </summary>
    [Table("Warehouses")]
    public class Warehouse
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// Уникальный идентификатор склада (формат WH000001)
        /// Генерируется базой данных автоматически
        /// </summary>
        [Column("Id")]
        [StringLength(10)]
        [Required]
        [Display(Name = "ID склада", Description = "Уникальный идентификатор склада в формате WH000001")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Наименование склада
        /// </summary>
        [Column("Name")]
        [StringLength(100)]
        [Required]
        [Display(Name = "Наименование", Description = "Полное наименование склада")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// ID отдела, к которому относится склад
        /// </summary>
        [Column("DepartmentId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Отдел", Description = "Ссылка на отдел, к которому относится склад")]
        public string DepartmentId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к отделу
        /// </summary>
        [ForeignKey("DepartmentId")]
        [Display(Name = "Отдел", Description = "Детали отдела")]
        public Department? Department { get; set; }

        /// <summary>
        /// Адрес склада
        /// </summary>
        [Column("Address")]
        [StringLength(500)]
        [Display(Name = "Адрес", Description = "Физический адрес склада")]
        public string? Address { get; set; }

        /// <summary>
        /// Флаг активности склада
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Признак активности склада (может использоваться в заявках)")]
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