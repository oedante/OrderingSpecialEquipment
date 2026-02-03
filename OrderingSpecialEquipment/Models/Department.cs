using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель отдела организации
    /// </summary>
    [Table("Departments")]
    public class Department
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// Уникальный идентификатор отдела (формат DE000001)
        /// Генерируется базой данных автоматически
        /// </summary>
        [Column("Id")]
        [StringLength(10)]
        [Required]
        [Display(Name = "ID отдела", Description = "Уникальный идентификатор отдела в формате DE000001")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Наименование отдела
        /// </summary>
        [Column("Name")]
        [StringLength(100)]
        [Required]
        [Display(Name = "Наименование", Description = "Полное наименование отдела")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Флаг активности отдела
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Признак активности отдела (может использоваться в заявках)")]
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