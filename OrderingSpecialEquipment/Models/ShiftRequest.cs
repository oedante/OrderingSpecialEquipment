using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы заявок на технику.
    /// </summary>
    [Table("ShiftRequests")]
    public class ShiftRequest
    {
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Уникальный ключ заявки")]
        public int Key { get; set; } // SERIAL

        [Column("Date")]
        [Display(Name = "Дата", Description = "Дата смены")]
        public DateTime Date { get; set; }

        [Column("Shift")]
        [Display(Name = "Смена", Description = "Тип смены: 0 - ночь, 1 - день")]
        public int Shift { get; set; } // 0 - ночь, 1 - день

        [Column("EquipmentId")]
        [StringLength(10)]
        [Display(Name = "ID техники", Description = "ID заказанной техники")]
        public string EquipmentId { get; set; } = string.Empty;

        [Column("LicensePlateId")]
        [StringLength(10)]
        [Display(Name = "ID госномера", Description = "ID госномера техники (если известен)")]
        public string? LicensePlateId { get; set; }

        [Column("WarehouseId")]
        [StringLength(10)]
        [Display(Name = "ID склада", Description = "ID склада, где будет работать техника")]
        public string WarehouseId { get; set; } = string.Empty;

        [Column("AreaId")]
        [StringLength(10)]
        [Display(Name = "ID территории", Description = "ID территории склада (если указана)")]
        public string? AreaId { get; set; }

        [Column("VehicleNumber")]
        [StringLength(50)]
        [Display(Name = "Номер техники", Description = "Фактический номер техники (госномер, если неизвестен в справочнике)")]
        public string? VehicleNumber { get; set; }

        [Column("VehicleBrand")]
        [StringLength(50)]
        [Display(Name = "Марка техники", Description = "Фактическая марка техники")]
        public string? VehicleBrand { get; set; }

        [Column("LessorOrganizationId")]
        [StringLength(10)]
        [Display(Name = "ID арендодателя", Description = "ID организации-арендодателя (фактической)")]
        public string? LessorOrganizationId { get; set; }

        [Column("RequestedCount")]
        [Display(Name = "Запрошено", Description = "Количество единиц техники, запрошенных в заявке")]
        public int RequestedCount { get; set; } = 1;

        [Column("WorkedHours")]
        [Display(Name = "Отработано часов", Description = "Количество часов, отработанных техникой")]
        public decimal? WorkedHours { get; set; }

        [Column("ActualCost")]
        [Display(Name = "Фактическая стоимость", Description = "Фактическая стоимость за отработанное время")]
        public decimal? ActualCost { get; set; }

        [Column("IsWorked")]
        [Display(Name = "Выполнено", Description = "Вышла ли техника на работу")]
        public bool IsWorked { get; set; } = false;

        [Column("IsBlocked")]
        [Display(Name = "Заблокировано", Description = "Заблокирована ли заявка (не может быть изменена)")]
        public bool IsBlocked { get; set; } = false;

        [Column("Comment")]
        [Display(Name = "Комментарий", Description = "Произвольный комментарий к заявке")]
        public string? Comment { get; set; }

        [Column("CreatedByUserId")]
        [StringLength(10)]
        [Display(Name = "ID создавшего", Description = "ID пользователя, создавшего заявку")]
        public string CreatedByUserId { get; set; } = string.Empty;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания заявки")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("DepartmentId")]
        [StringLength(10)]
        [Display(Name = "ID отдела", Description = "ID отдела, подавшего заявку")]
        public string? DepartmentId { get; set; }

        [Column("ProgramYear")]
        [Display(Name = "Год программы", Description = "Год, к которому относится заявка в контексте транспортной программы")]
        public int? ProgramYear { get; set; }

        [Column("ProgramMonth")]
        [Display(Name = "Месяц программы", Description = "Месяц, к которому относится заявка в контексте транспортной программы")]
        public int? ProgramMonth { get; set; }

        // Навигационные свойства (опционально, для EF)
        public virtual Equipment Equipment { get; set; } = null!;
        public virtual LicensePlate? LicensePlate { get; set; }
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual WarehouseArea? Area { get; set; }
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual Department? Department { get; set; }
        public virtual LessorOrganization? LessorOrganization { get; set; }
    }
}