using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель специальной техники и оборудования
    /// </summary>
    [Table("Equipments")]
    public class Equipment
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// Уникальный идентификатор техники (формат EQ000001)
        /// Генерируется базой данных автоматически
        /// </summary>
        [Column("Id")]
        [StringLength(10)]
        [Required]
        [Display(Name = "ID техники", Description = "Уникальный идентификатор техники в формате EQ000001")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Наименование техники
        /// </summary>
        [Column("Name")]
        [StringLength(200)]
        [Required]
        [Display(Name = "Наименование", Description = "Полное наименование техники или оборудования")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Категория техники (Спецтехника, Рабочий, Оборудование)
        /// </summary>
        [Column("Category")]
        [StringLength(50)]
        [Display(Name = "Категория", Description = "Категория техники для группировки")]
        public string? Category { get; set; }

        /// <summary>
        /// Возможность заказа нескольких единиц в одной заявке
        /// </summary>
        [Column("CanOrderMultiple")]
        [Display(Name = "Можно заказать несколько", Description = "Разрешено ли указывать количество >1 в одной заявке (для рабочих - да, для спецтехники - нет)")]
        [DefaultValue(false)]
        public bool CanOrderMultiple { get; set; } = false;

        /// <summary>
        /// Стоимость часа работы
        /// </summary>
        [Column("HourlyCost")]
        [Display(Name = "Стоимость часа", Description = "Стоимость одного часа работы техники в рублях")]
        [DataType(DataType.Currency)]
        public decimal? HourlyCost { get; set; }

        /// <summary>
        /// Требуется ли оператор для работы
        /// </summary>
        [Column("RequiresOperator")]
        [Display(Name = "Требует оператора", Description = "Требуется ли квалифицированный оператор для управления техникой")]
        [DefaultValue(false)]
        public bool RequiresOperator { get; set; } = false;

        /// <summary>
        /// Дополнительное описание
        /// </summary>
        [Column("Description")]
        [StringLength(500)]
        [Display(Name = "Описание", Description = "Дополнительная информация о технике")]
        public string? Description { get; set; }

        /// <summary>
        /// Флаг активности техники
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активна", Description = "Признак активности техники (может использоваться в заявках)")]
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