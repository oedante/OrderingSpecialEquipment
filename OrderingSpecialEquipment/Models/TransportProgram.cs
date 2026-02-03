using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель транспортной программы отдела на год
    /// </summary>
    [Table("TransportProgram")]
    public class TransportProgram
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// ID отдела
        /// </summary>
        [Column("DepartmentId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Отдел", Description = "Ссылка на отдел")]
        public string DepartmentId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к отделу
        /// </summary>
        [ForeignKey("DepartmentId")]
        [Display(Name = "Отдел", Description = "Детали отдела")]
        public Department? Department { get; set; }

        /// <summary>
        /// Год программы
        /// </summary>
        [Column("Year")]
        [Required]
        [Display(Name = "Год", Description = "Год транспортной программы")]
        [Range(2020, 2100)]
        public int Year { get; set; }

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
        /// Стоимость часа работы (актуальная на момент создания программы)
        /// </summary>
        [Column("HourlyCost")]
        [Required]
        [Display(Name = "Стоимость часа", Description = "Стоимость одного часа работы техники в рублях")]
        [DataType(DataType.Currency)]
        public decimal HourlyCost { get; set; }

        // ========== Плановые часы по месяцам ==========

        [Column("JanuaryHours")]
        [Display(Name = "Январь", Description = "Плановые часы работы в январе")]
        [Range(0, double.MaxValue)]
        [DefaultValue(0)]
        public decimal JanuaryHours { get; set; } = 0;

        [Column("FebruaryHours")]
        [Display(Name = "Февраль", Description = "Плановые часы работы в феврале")]
        [Range(0, double.MaxValue)]
        [DefaultValue(0)]
        public decimal FebruaryHours { get; set; } = 0;

        [Column("MarchHours")]
        [Display(Name = "Март", Description = "Плановые часы работы в марте")]
        [Range(0, double.MaxValue)]
        [DefaultValue(0)]
        public decimal MarchHours { get; set; } = 0;

        [Column("AprilHours")]
        [Display(Name = "Апрель", Description = "Плановые часы работы в апреле")]
        [Range(0, double.MaxValue)]
        [DefaultValue(0)]
        public decimal AprilHours { get; set; } = 0;

        [Column("MayHours")]
        [Display(Name = "Май", Description = "Плановые часы работы в мае")]
        [Range(0, double.MaxValue)]
        [DefaultValue(0)]
        public decimal MayHours { get; set; } = 0;

        [Column("JuneHours")]
        [Display(Name = "Июнь", Description = "Плановые часы работы в июне")]
        [Range(0, double.MaxValue)]
        [DefaultValue(0)]
        public decimal JuneHours { get; set; } = 0;

        [Column("JulyHours")]
        [Display(Name = "Июль", Description = "Плановые часы работы в июле")]
        [Range(0, double.MaxValue)]
        [DefaultValue(0)]
        public decimal JulyHours { get; set; } = 0;

        [Column("AugustHours")]
        [Display(Name = "Август", Description = "Плановые часы работы в августе")]
        [Range(0, double.MaxValue)]
        [DefaultValue(0)]
        public decimal AugustHours { get; set; } = 0;

        [Column("SeptemberHours")]
        [Display(Name = "Сентябрь", Description = "Плановые часы работы в сентябре")]
        [Range(0, double.MaxValue)]
        [DefaultValue(0)]
        public decimal SeptemberHours { get; set; } = 0;

        [Column("OctoberHours")]
        [Display(Name = "Октябрь", Description = "Плановые часы работы в октябре")]
        [Range(0, double.MaxValue)]
        [DefaultValue(0)]
        public decimal OctoberHours { get; set; } = 0;

        [Column("NovemberHours")]
        [Display(Name = "Ноябрь", Description = "Плановые часы работы в ноябре")]
        [Range(0, double.MaxValue)]
        [DefaultValue(0)]
        public decimal NovemberHours { get; set; } = 0;

        [Column("DecemberHours")]
        [Display(Name = "Декабрь", Description = "Плановые часы работы в декабре")]
        [Range(0, double.MaxValue)]
        [DefaultValue(0)]
        public decimal DecemberHours { get; set; } = 0;

        // ========== Расчетные поля (хранятся в БД) ==========

        [Column("TotalYearHours")]
        [Display(Name = "Всего часов в году", Description = "Суммарное количество часов по году")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal TotalYearHours { get; set; }

        [Column("TotalYearCost")]
        [Display(Name = "Общая стоимость", Description = "Общая стоимость работы техники за год")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal TotalYearCost { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Возвращает плановые часы для указанного месяца
        /// </summary>
        public decimal GetHoursForMonth(int month)
        {
            return month switch
            {
                1 => JanuaryHours,
                2 => FebruaryHours,
                3 => MarchHours,
                4 => AprilHours,
                5 => MayHours,
                6 => JuneHours,
                7 => JulyHours,
                8 => AugustHours,
                9 => SeptemberHours,
                10 => OctoberHours,
                11 => NovemberHours,
                12 => DecemberHours,
                _ => 0
            };
        }

        /// <summary>
        /// Устанавливает плановые часы для указанного месяца
        /// </summary>
        public void SetHoursForMonth(int month, decimal hours)
        {
            switch (month)
            {
                case 1: JanuaryHours = hours; break;
                case 2: FebruaryHours = hours; break;
                case 3: MarchHours = hours; break;
                case 4: AprilHours = hours; break;
                case 5: MayHours = hours; break;
                case 6: JuneHours = hours; break;
                case 7: JulyHours = hours; break;
                case 8: AugustHours = hours; break;
                case 9: SeptemberHours = hours; break;
                case 10: OctoberHours = hours; break;
                case 11: NovemberHours = hours; break;
                case 12: DecemberHours = hours; break;
            }
        }
    }
}