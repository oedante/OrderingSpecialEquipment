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
        public string Id { get; set; } = string.Empty;

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
        public string Name { get; set; } = string.Empty;

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
        /// Список складов, к которым относится эта территория (через связь многие-ко-многим)
        /// </summary>
        public virtual ICollection<WarehouseAreaLink> WarehouseLinks { get; set; }

        /// <summary>
        /// Список складов (для удобства доступа)
        /// </summary>
        [NotMapped]
        public IEnumerable<Warehouse> Warehouses => WarehouseLinks?.Select(wl => wl.Warehouse) ?? new List<Warehouse>();

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
            WarehouseLinks = new HashSet<WarehouseAreaLink>();
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