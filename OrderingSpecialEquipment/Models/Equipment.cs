using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель техники/оборудования
    /// Таблица Equipments в базе данных
    /// </summary>
    [Table("Equipments")]
    [Display(Name = "Техника", Description = "Справочник техники и оборудования")]
    public class Equipment
    {
        #region Свойства

        /// <summary>
        /// Уникальный идентификатор техники (первичный ключ)
        /// Формат: EQ000001
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Display(Name = "Идентификатор", Description = "Уникальный идентификатор техники")]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Числовой ключ (автоинкремент)
        /// </summary>
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Числовой автоинкрементный ключ")]
        public int Key { get; set; }

        /// <summary>
        /// Наименование техники
        /// </summary>
        [Required(ErrorMessage = "Наименование техники обязательно")]
        [Column("Name")]
        [Display(Name = "Наименование", Description = "Наименование техники")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Категория техники
        /// </summary>
        [Column("Category")]
        [Display(Name = "Категория", Description = "Категория техники")]
        [StringLength(50)]
        public string? Category { get; set; }

        /// <summary>
        /// Можно ли заказать несколько единиц в одной заявке
        /// </summary>
        [Column("CanOrderMultiple")]
        [Display(Name = "Несколько единиц", Description = "Можно ли заказать несколько единиц в одной заявке")]
        [DefaultValue(false)]
        public bool CanOrderMultiple { get; set; }

        /// <summary>
        /// Почасовая стоимость
        /// </summary>
        [Column("HourlyCost")]
        [Display(Name = "Стоимость часа", Description = "Почасовая стоимость аренды")]
        [DataType(DataType.Currency)]
        public decimal? HourlyCost { get; set; }

        /// <summary>
        /// Требуется ли оператор для работы
        /// </summary>
        [Column("RequiresOperator")]
        [Display(Name = "Требуется оператор", Description = "Требуется ли оператор для работы")]
        [DefaultValue(false)]
        public bool RequiresOperator { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [Column("Description")]
        [Display(Name = "Описание", Description = "Описание техники")]
        [StringLength(500)]
        public string? Description { get; set; }  // Сделано nullable

        /// <summary>
        /// Активна ли техника
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активна", Description = "Признак активности техники")]
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
        /// Список госномеров этой техники
        /// </summary>
        public virtual ICollection<LicensePlate> LicensePlates { get; set; }

        /// <summary>
        /// Список заявок на эту технику
        /// </summary>
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; }

        /// <summary>
        /// Список записей транспортной программы для этой техники
        /// </summary>
        public virtual ICollection<TransportProgram> TransportPrograms { get; set; }

        /// <summary>
        /// Список зависимостей, где эта техника является основной
        /// </summary>
        public virtual ICollection<EquipmentDependency> MainEquipmentDependencies { get; set; }

        /// <summary>
        /// Список зависимостей, где эта техника является зависимой
        /// </summary>
        public virtual ICollection<EquipmentDependency> DependentEquipmentDependencies { get; set; }

        /// <summary>
        /// Список избранного для пользователей
        /// </summary>
        public virtual ICollection<UserFavorite> UserFavorites { get; set; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Equipment()
        {
            LicensePlates = new HashSet<LicensePlate>();
            ShiftRequests = new HashSet<ShiftRequest>();
            TransportPrograms = new HashSet<TransportProgram>();
            MainEquipmentDependencies = new HashSet<EquipmentDependency>();
            DependentEquipmentDependencies = new HashSet<EquipmentDependency>();
            UserFavorites = new HashSet<UserFavorite>();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Возвращает строковое представление техники
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({Id})";
        }

        #endregion
    }
}