using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель организации-арендодателя
    /// Таблица LessorOrganizations в базе данных
    /// </summary>
    [Table("LessorOrganizations")]
    [Display(Name = "Арендодатель", Description = "Организация-арендодатель техники")]
    public class LessorOrganization
    {
        #region Свойства

        /// <summary>
        /// Уникальный идентификатор организации (первичный ключ)
        /// Формат: LO000001
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Display(Name = "Идентификатор", Description = "Уникальный идентификатор организации")]
        [StringLength(10)]
        public string Id { get; set; }

        /// <summary>
        /// Числовой ключ (автоинкремент)
        /// </summary>
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Числовой автоинкрементный ключ")]
        public int Key { get; set; }

        /// <summary>
        /// Наименование организации
        /// </summary>
        [Required(ErrorMessage = "Наименование организации обязательно")]
        [Column("Name")]
        [Display(Name = "Наименование", Description = "Наименование организации")]
        [StringLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// ИНН организации
        /// </summary>
        [Column("INN")]
        [Display(Name = "ИНН", Description = "Идентификационный номер налогоплательщика")]
        [StringLength(12)]
        public string INN { get; set; }

        /// <summary>
        /// Контактное лицо
        /// </summary>
        [Column("ContactPerson")]
        [Display(Name = "Контактное лицо", Description = "Контактное лицо организации")]
        [StringLength(150)]
        public string ContactPerson { get; set; }

        /// <summary>
        /// Телефон
        /// </summary>
        [Column("Phone")]
        [Display(Name = "Телефон", Description = "Контактный телефон")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(20)]
        public string Phone { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [Column("Email")]
        [Display(Name = "Email", Description = "Адрес электронной почты")]
        [DataType(DataType.EmailAddress)]
        [StringLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// Адрес организации
        /// </summary>
        [Column("Address")]
        [Display(Name = "Адрес", Description = "Физический адрес организации")]
        [StringLength(500)]
        public string Address { get; set; }

        /// <summary>
        /// Активна ли организация
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активна", Description = "Признак активности организации")]
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
        /// Список госномеров этой организации
        /// </summary>
        public virtual ICollection<LicensePlate> LicensePlates { get; set; }

        /// <summary>
        /// Список заявок с этой организацией
        /// </summary>
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public LessorOrganization()
        {
            LicensePlates = new HashSet<LicensePlate>();
            ShiftRequests = new HashSet<ShiftRequest>();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Возвращает строковое представление организации
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({Id})";
        }

        #endregion
    }
}