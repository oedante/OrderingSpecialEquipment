using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель государственного номера техники
    /// Таблица LicensePlates в базе данных
    /// </summary>
    [Table("LicensePlates")]
    [Display(Name = "Госномер", Description = "Государственный номер техники")]
    public class LicensePlate
    {
        #region Свойства

        /// <summary>
        /// Уникальный идентификатор госномера (первичный ключ)
        /// Формат: LP000001
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Display(Name = "Идентификатор", Description = "Уникальный идентификатор госномера")]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Числовой ключ (автоинкремент)
        /// </summary>
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Числовой автоинкрементный ключ")]
        public int Key { get; set; }

        /// <summary>
        /// Государственный номер
        /// </summary>
        [Required(ErrorMessage = "Государственный номер обязателен")]
        [Column("PlateNumber")]
        [Display(Name = "Госномер", Description = "Государственный номер техники")]
        [StringLength(20)]
        public string PlateNumber { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор техники
        /// </summary>
        [Required(ErrorMessage = "Техника обязательна")]
        [Column("EquipmentId")]
        [Display(Name = "Техника", Description = "Идентификатор техники")]
        [StringLength(10)]
        public string EquipmentId { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор организации-арендодателя
        /// </summary>
        [Required(ErrorMessage = "Организация-арендодатель обязательна")]
        [Column("LessorOrganizationId")]
        [Display(Name = "Арендодатель", Description = "Идентификатор организации-арендодателя")]
        [StringLength(10)]
        public string LessorOrganizationId { get; set; } = string.Empty;

        /// <summary>
        /// Марка техники
        /// </summary>
        [Column("Brand")]
        [Display(Name = "Марка", Description = "Марка техники")]
        [StringLength(100)]
        public string? Brand { get; set; }

        /// <summary>
        /// Год выпуска
        /// </summary>
        [Column("Year")]
        [Display(Name = "Год выпуска", Description = "Год выпуска техники")]
        [Range(1900, 2100)]
        public int? Year { get; set; }

        /// <summary>
        /// Грузоподъемность/емкость
        /// </summary>
        [Column("Capacity")]
        [Display(Name = "Грузоподъемность", Description = "Грузоподъемность или емкость")]
        [StringLength(50)]
        public string? Capacity { get; set; }

        /// <summary>
        /// VIN номер
        /// </summary>
        [Column("VIN")]
        [Display(Name = "VIN", Description = "Идентификационный номер транспортного средства")]
        [StringLength(50)]
        public string? VIN { get; set; }

        /// <summary>
        /// Активен ли госномер
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Признак активности госномера")]
        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        #endregion

        #region Вычисляемые свойства

        /// <summary>
        /// Отображение госномера для ComboBox
        /// </summary>
        [NotMapped]
        public string PlateDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(Brand))
                    return $"{PlateNumber} - {Brand}";
                return PlateNumber;
            }
        }

        #endregion

        #region Навигационные свойства

        /// <summary>
        /// Техника
        /// </summary>
        [ForeignKey("EquipmentId")]
        public virtual Equipment? Equipment { get; set; }

        /// <summary>
        /// Организация-арендодатель
        /// </summary>
        [ForeignKey("LessorOrganizationId")]
        public virtual LessorOrganization? LessorOrganization { get; set; }

        /// <summary>
        /// Список заявок с этим госномером
        /// </summary>
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public LicensePlate()
        {
            ShiftRequests = new HashSet<ShiftRequest>();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Возвращает строковое представление госномера
        /// </summary>
        public override string ToString()
        {
            return PlateDisplay;
        }

        #endregion
    }
}