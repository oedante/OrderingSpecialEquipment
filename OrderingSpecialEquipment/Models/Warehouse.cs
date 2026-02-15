using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель склада
    /// Таблица Warehouses в базе данных
    /// </summary>
    [Table("Warehouses")]
    [Display(Name = "Склад", Description = "Склад предприятия")]
    public class Warehouse
    {
        #region Свойства

        /// <summary>
        /// Уникальный идентификатор склада (первичный ключ)
        /// Формат: WH000001
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Display(Name = "Идентификатор", Description = "Уникальный идентификатор склада")]
        [StringLength(10)]
        public string Id { get; set; }

        /// <summary>
        /// Числовой ключ (автоинкремент)
        /// </summary>
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Числовой автоинкрементный ключ")]
        public int Key { get; set; }

        /// <summary>
        /// Наименование склада
        /// </summary>
        [Required]
        [Column("Name")]
        [Display(Name = "Наименование", Description = "Наименование склада")]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Идентификатор отдела
        /// </summary>
        [Required]
        [Column("DepartmentId")]
        [Display(Name = "Отдел", Description = "Идентификатор отдела, к которому относится склад")]
        [StringLength(10)]
        public string DepartmentId { get; set; }

        /// <summary>
        /// Адрес склада
        /// </summary>
        [Column("Address")]
        [Display(Name = "Адрес", Description = "Физический адрес склада")]
        [StringLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Активен ли склад
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Признак активности склада")]
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
        /// Отдел
        /// </summary>
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        /// <summary>
        /// Список территорий склада
        /// </summary>
        public virtual ICollection<WarehouseArea> WarehouseAreas { get; set; }

        /// <summary>
        /// Список заявок на этом складе
        /// </summary>
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; }

        /// <summary>
        /// Список доступов пользователей к этому складу
        /// </summary>
        public virtual ICollection<UserWarehouseAccess> UserWarehouseAccesses { get; set; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Warehouse()
        {
            WarehouseAreas = new HashSet<WarehouseArea>();
            ShiftRequests = new HashSet<ShiftRequest>();
            UserWarehouseAccesses = new HashSet<UserWarehouseAccess>();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Возвращает строковое представление склада
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({Id})";
        }

        #endregion
    }
}