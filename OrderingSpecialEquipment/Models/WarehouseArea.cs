using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель территории склада
    /// Таблица WarehouseAreas в базе данных
    /// </summary>
    [Table("WarehouseAreas")]
    [Display(Name = "Территория склада", Description = "Территория внутри склада")]
    public class WarehouseArea
    {
        #region Свойства

        /// <summary>
        /// Уникальный идентификатор территории (первичный ключ)
        /// Формат: WA000001
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Display(Name = "Идентификатор", Description = "Уникальный идентификатор территории")]
        [StringLength(10)]
        public string Id { get; set; }

        /// <summary>
        /// Числовой ключ (автоинкремент)
        /// </summary>
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Числовой автоинкрементный ключ")]
        public int Key { get; set; }

        /// <summary>
        /// Наименование территории
        /// </summary>
        [Required]
        [Column("Name")]
        [Display(Name = "Наименование", Description = "Наименование территории")]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Идентификатор склада
        /// </summary>
        [Required]
        [Column("WarehouseId")]
        [Display(Name = "Склад", Description = "Идентификатор склада")]
        [StringLength(10)]
        public string WarehouseId { get; set; }

        /// <summary>
        /// Тип территории
        /// </summary>
        [Column("AreaType")]
        [Display(Name = "Тип территории", Description = "Тип территории (разгрузка, хранение и т.д.)")]
        [StringLength(50)]
        public string? AreaType { get; set; }

        /// <summary>
        /// Максимальная вместимость
        /// </summary>
        [Column("MaxCapacity")]
        [Display(Name = "Макс. вместимость", Description = "Максимальная вместимость территории")]
        public int? MaxCapacity { get; set; }

        /// <summary>
        /// Активна ли территория
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активна", Description = "Признак активности территории")]
        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        #endregion

        #region Навигационные свойства

        /// <summary>
        /// Склад
        /// </summary>
        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }

        /// <summary>
        /// Список заявок на этой территории
        /// </summary>
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public WarehouseArea()
        {
            ShiftRequests = new HashSet<ShiftRequest>();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Возвращает строковое представление территории
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({Id})";
        }

        #endregion
    }
}