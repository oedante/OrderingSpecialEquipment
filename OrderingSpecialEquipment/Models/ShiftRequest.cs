using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель заявки на технику для смены
    /// </summary>
    [Table("ShiftRequests")]
    public class ShiftRequest
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// Дата заявки
        /// </summary>
        [Column("Date")]
        [Required]
        [Display(Name = "Дата", Description = "Дата, на которую подана заявка")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        /// <summary>
        /// Смена (0 - Ночная, 1 - Дневная)
        /// </summary>
        [Column("Shift")]
        [Required]
        [Display(Name = "Смена", Description = "0 - Ночная смена, 1 - Дневная смена")]
        [Range(0, 1)]
        public int Shift { get; set; }

        /// <summary>
        /// ID техники
        /// </summary>
        [Column("EquipmentId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Техника", Description = "Ссылка на технику")]
        public string EquipmentId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к технике
        /// </summary>
        [ForeignKey("EquipmentId")]
        [Display(Name = "Техника", Description = "Детали техники")]
        public Equipment? Equipment { get; set; }

        /// <summary>
        /// ID госномера (если известен)
        /// </summary>
        [Column("LicensePlateId")]
        [StringLength(10)]
        [Display(Name = "Госномер", Description = "Ссылка на госномер техники")]
        public string? LicensePlateId { get; set; }

        /// <summary>
        /// Навигационное свойство к госномеру
        /// </summary>
        [ForeignKey("LicensePlateId")]
        [Display(Name = "Госномер", Description = "Детали госномера")]
        public LicensePlate? LicensePlate { get; set; }

        /// <summary>
        /// ID склада
        /// </summary>
        [Column("WarehouseId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Склад", Description = "Ссылка на склад")]
        public string WarehouseId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к складу
        /// </summary>
        [ForeignKey("WarehouseId")]
        [Display(Name = "Склад", Description = "Детали склада")]
        public Warehouse? Warehouse { get; set; }

        /// <summary>
        /// ID территории склада
        /// </summary>
        [Column("AreaId")]
        [StringLength(10)]
        [Display(Name = "Территория", Description = "Ссылка на территорию склада")]
        public string? AreaId { get; set; }

        /// <summary>
        /// Навигационное свойство к территории
        /// </summary>
        [ForeignKey("AreaId")]
        [Display(Name = "Территория", Description = "Детали территории")]
        public WarehouseArea? Area { get; set; }

        /// <summary>
        /// Государственный номер (вводится пользователем)
        /// </summary>
        [Column("VehicleNumber")]
        [StringLength(50)]
        [Display(Name = "Госномер (введенный)", Description = "Госномер, введенный пользователем")]
        public string? VehicleNumber { get; set; }

        /// <summary>
        /// Марка техники (вводится пользователем)
        /// </summary>
        [Column("VehicleBrand")]
        [StringLength(50)]
        [Display(Name = "Марка (введенная)", Description = "Марка техники, введенная пользователем")]
        public string? VehicleBrand { get; set; }

        /// <summary>
        /// ID организации-арендодателя
        /// </summary>
        [Column("LessorOrganizationId")]
        [StringLength(10)]
        [Display(Name = "Организация", Description = "Ссылка на организацию-арендодателя")]
        public string? LessorOrganizationId { get; set; }

        /// <summary>
        /// Навигационное свойство к организации
        /// </summary>
        [ForeignKey("LessorOrganizationId")]
        [Display(Name = "Организация", Description = "Детали организации-арендодателя")]
        public LessorOrganization? LessorOrganization { get; set; }

        /// <summary>
        /// Запрошенное количество единиц техники
        /// </summary>
        [Column("RequestedCount")]
        [Required]
        [Display(Name = "Количество", Description = "Запрошенное количество единиц техники")]
        [Range(1, int.MaxValue)]
        [DefaultValue(1)]
        public int RequestedCount { get; set; } = 1;

        /// <summary>
        /// Фактически отработанные часы
        /// </summary>
        [Column("WorkedHours")]
        [Display(Name = "Отработано часов", Description = "Фактически отработанное количество часов")]
        [Range(0, 24)]
        public decimal? WorkedHours { get; set; }

        /// <summary>
        /// Фактическая стоимость
        /// </summary>
        [Column("ActualCost")]
        [Display(Name = "Фактическая стоимость", Description = "Фактическая стоимость работы техники")]
        [DataType(DataType.Currency)]
        public decimal? ActualCost { get; set; }

        /// <summary>
        /// Флаг фактической работы техники
        /// </summary>
        [Column("IsWorked")]
        [Display(Name = "Отработала", Description = "Признак того, что техника фактически отработала")]
        [DefaultValue(false)]
        public bool IsWorked { get; set; } = false;

        /// <summary>
        /// Флаг блокировки заявки (запрет редактирования)
        /// </summary>
        [Column("IsBlocked")]
        [Display(Name = "Заблокирована", Description = "Признак блокировки заявки от редактирования")]
        [DefaultValue(false)]
        public bool IsBlocked { get; set; } = false;

        /// <summary>
        /// Комментарий к заявке
        /// </summary>
        [Column("Comment")]
        [Display(Name = "Комментарий", Description = "Дополнительный комментарий к заявке")]
        public string? Comment { get; set; }

        /// <summary>
        /// ID пользователя, создавшего заявку
        /// </summary>
        [Column("CreatedByUserId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Создал", Description = "Ссылка на пользователя, создавшего заявку")]
        public string CreatedByUserId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к пользователю-создателю
        /// </summary>
        [ForeignKey("CreatedByUserId")]
        [Display(Name = "Создал", Description = "Детали пользователя-создателя")]
        public User? CreatedByUser { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID отдела (для связи с транспортной программой)
        /// </summary>
        [Column("DepartmentId")]
        [StringLength(10)]
        [Display(Name = "Отдел", Description = "Ссылка на отдел для связи с транспортной программой")]
        public string? DepartmentId { get; set; }

        /// <summary>
        /// Навигационное свойство к отделу
        /// </summary>
        [ForeignKey("DepartmentId")]
        [Display(Name = "Отдел", Description = "Детали отдела")]
        public Department? Department { get; set; }

        /// <summary>
        /// Год транспортной программы
        /// </summary>
        [Column("ProgramYear")]
        [Display(Name = "Год программы", Description = "Год транспортной программы")]
        [Range(2020, 2100)]
        public int? ProgramYear { get; set; }

        /// <summary>
        /// Месяц транспортной программы (1-12)
        /// </summary>
        [Column("ProgramMonth")]
        [Display(Name = "Месяц программы", Description = "Месяц транспортной программы (1-12)")]
        [Range(1, 12)]
        public int? ProgramMonth { get; set; }
    }
}