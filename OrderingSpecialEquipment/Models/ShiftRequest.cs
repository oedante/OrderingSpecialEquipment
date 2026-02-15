using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель заявки на технику
    /// Таблица ShiftRequests в базе данных
    /// </summary>
    [Table("ShiftRequests")]
    [Display(Name = "Заявка", Description = "Заявка на специальную технику")]
    public class ShiftRequest
    {
        #region Свойства

        /// <summary>
        /// Числовой ключ (первичный ключ)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Числовой автоинкрементный ключ")]
        public int Key { get; set; }

        /// <summary>
        /// Дата заявки
        /// </summary>
        [Required]
        [Column("Date")]
        [Display(Name = "Дата", Description = "Дата заявки")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        /// <summary>
        /// Смена (0 - дневная, 1 - ночная)
        /// </summary>
        [Required]
        [Column("Shift")]
        [Display(Name = "Смена", Description = "0 - дневная смена (07:30-18:30), 1 - ночная смена (19:30-06:30)")]
        [Range(0, 1)]
        public int Shift { get; set; }

        /// <summary>
        /// Идентификатор техники
        /// </summary>
        [Required]
        [Column("EquipmentId")]
        [Display(Name = "Техника", Description = "Идентификатор техники")]
        [StringLength(10)]
        public string EquipmentId { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор госномера
        /// </summary>
        [Column("LicensePlateId")]
        [Display(Name = "Госномер", Description = "Идентификатор государственного номера")]
        [StringLength(10)]
        public string? LicensePlateId { get; set; }

        /// <summary>
        /// Идентификатор склада
        /// </summary>
        [Required]
        [Column("WarehouseId")]
        [Display(Name = "Склад", Description = "Идентификатор склада")]
        [StringLength(10)]
        public string WarehouseId { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор территории склада
        /// </summary>
        [Column("AreaId")]
        [Display(Name = "Территория", Description = "Идентификатор территории склада")]
        [StringLength(10)]
        public string? AreaId { get; set; }

        /// <summary>
        /// Номер транспортного средства
        /// </summary>
        [Column("VehicleNumber")]
        [Display(Name = "Номер ТС", Description = "Номер транспортного средства")]
        [StringLength(50)]
        public string? VehicleNumber { get; set; }

        /// <summary>
        /// Марка транспортного средства
        /// </summary>
        [Column("VehicleBrand")]
        [Display(Name = "Марка ТС", Description = "Марка транспортного средства")]
        [StringLength(50)]
        public string? VehicleBrand { get; set; }

        /// <summary>
        /// Идентификатор организации-арендодателя
        /// </summary>
        [Column("LessorOrganizationId")]
        [Display(Name = "Арендодатель", Description = "Идентификатор организации-арендодателя")]
        [StringLength(10)]
        public string? LessorOrganizationId { get; set; }

        /// <summary>
        /// Запрошенное количество
        /// </summary>
        [Column("RequestedCount")]
        [Display(Name = "Количество", Description = "Запрошенное количество единиц")]
        [DefaultValue(1)]
        public int RequestedCount { get; set; } = 1;

        /// <summary>
        /// Отработанные часы
        /// </summary>
        [Column("WorkedHours")]
        [Display(Name = "Отработано часов", Description = "Фактически отработанные часы")]
        public decimal? WorkedHours { get; set; }

        /// <summary>
        /// Фактическая стоимость
        /// </summary>
        [Column("ActualCost")]
        [Display(Name = "Факт. стоимость", Description = "Фактическая стоимость")]
        [DataType(DataType.Currency)]
        public decimal? ActualCost { get; set; }

        /// <summary>
        /// Отработано ли (флаг)
        /// </summary>
        [Column("IsWorked")]
        [Display(Name = "Отработано", Description = "Признак того, что техника отработала")]
        [DefaultValue(false)]
        public bool IsWorked { get; set; }

        /// <summary>
        /// Заблокирована ли запись
        /// </summary>
        [Column("IsBlocked")]
        [Display(Name = "Заблокирована", Description = "Признак блокировки записи")]
        [DefaultValue(false)]
        public bool IsBlocked { get; set; }

        /// <summary>
        /// Техника не была предоставлена
        /// </summary>
        [Column("IsNotProvided")]
        [Display(Name = "Не предоставлена", Description = "Техника не была предоставлена арендодателем")]
        [DefaultValue(false)]
        public bool IsNotProvided { get; set; }

        /// <summary>
        /// Актировка (отмена по погодным условиям)
        /// </summary>
        [Column("IsWeatherCancellation")]
        [Display(Name = "Актировка", Description = "Отмена по погодным условиям")]
        [DefaultValue(false)]
        public bool IsWeatherCancellation { get; set; }

        /// <summary>
        /// Причина отмены
        /// </summary>
        [Column("CancellationReason")]
        [Display(Name = "Причина отмены", Description = "Причина отмены заявки")]
        [StringLength(200)]
        public string? CancellationReason { get; set; }

        /// <summary>
        /// ID пользователя, заблокировавшего запись
        /// </summary>
        [Column("LockedByUserId")]
        [Display(Name = "Заблокировал", Description = "ID пользователя, редактирующего запись")]
        [StringLength(10)]
        public string? LockedByUserId { get; set; }

        /// <summary>
        /// Время начала блокировки
        /// </summary>
        [Column("LockedAt")]
        [Display(Name = "Время блокировки", Description = "Время начала блокировки записи")]
        public DateTime? LockedAt { get; set; }

        /// <summary>
        /// Комментарий
        /// </summary>
        [Column("Comment")]
        [Display(Name = "Комментарий", Description = "Комментарий к заявке")]
        public string? Comment { get; set; }

        /// <summary>
        /// ID пользователя, создавшего заявку
        /// </summary>
        [Required]
        [Column("CreatedByUserId")]
        [Display(Name = "Создал", Description = "ID пользователя, создавшего заявку")]
        [StringLength(10)]
        public string CreatedByUserId { get; set; } = string.Empty;

        /// <summary>
        /// Дата создания заявки
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания заявки")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Идентификатор отдела
        /// </summary>
        [Column("DepartmentId")]
        [Display(Name = "Отдел", Description = "Идентификатор отдела")]
        [StringLength(10)]
        public string? DepartmentId { get; set; }

        #endregion

        #region Вычисляемые свойства

        /// <summary>
        /// Почасовая стоимость из связанной техники
        /// </summary>
        [NotMapped]
        public decimal? HourlyCost => Equipment?.HourlyCost;

        #endregion

        #region Навигационные свойства

        /// <summary>
        /// Техника
        /// </summary>
        [ForeignKey("EquipmentId")]
        public virtual Equipment? Equipment { get; set; }

        /// <summary>
        /// Госномер
        /// </summary>
        [ForeignKey("LicensePlateId")]
        public virtual LicensePlate? LicensePlate { get; set; }

        /// <summary>
        /// Склад
        /// </summary>
        [ForeignKey("WarehouseId")]
        public virtual Warehouse? Warehouse { get; set; }

        /// <summary>
        /// Территория склада
        /// </summary>
        [ForeignKey("AreaId")]
        public virtual WarehouseArea? Area { get; set; }

        /// <summary>
        /// Организация-арендодатель
        /// </summary>
        [ForeignKey("LessorOrganizationId")]
        public virtual LessorOrganization? LessorOrganization { get; set; }

        /// <summary>
        /// Пользователь, создавший заявку
        /// </summary>
        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedByUser { get; set; }

        /// <summary>
        /// Пользователь, заблокировавший заявку
        /// </summary>
        [ForeignKey("LockedByUserId")]
        public virtual User? LockedByUser { get; set; }

        /// <summary>
        /// Отдел
        /// </summary>
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        #endregion

        #region Методы

        /// <summary>
        /// Возвращает строковое представление заявки
        /// </summary>
        public override string ToString()
        {
            string shiftName = Shift == 0 ? "Дневная" : "Ночная";
            return $"Заявка от {Date:dd.MM.yyyy} {shiftName}";
        }

        /// <summary>
        /// Получить название смены
        /// </summary>
        public string GetShiftName()
        {
            return Shift == 0 ? "Дневная" : "Ночная";
        }

        #endregion
    }
}