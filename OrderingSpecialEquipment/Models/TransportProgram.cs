using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель транспортной программы
    /// Таблица TransportProgram в базе данных
    /// </summary>
    [Table("TransportProgram")]
    [Display(Name = "Транспортная программа", Description = "Плановые часы работы техники по отделам на год")]
    public class TransportProgram
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
        /// Идентификатор отдела
        /// </summary>
        [Required]
        [Column("DepartmentId")]
        [Display(Name = "Отдел", Description = "Идентификатор отдела")]
        [StringLength(10)]
        public string DepartmentId { get; set; }

        /// <summary>
        /// Год
        /// </summary>
        [Required]
        [Column("Year")]
        [Display(Name = "Год", Description = "Плановый год")]
        [Range(2020, 2100)]
        public int Year { get; set; }

        /// <summary>
        /// Идентификатор техники
        /// </summary>
        [Required]
        [Column("EquipmentId")]
        [Display(Name = "Техника", Description = "Идентификатор техники")]
        [StringLength(10)]
        public string EquipmentId { get; set; }

        /// <summary>
        /// Почасовая стоимость
        /// </summary>
        [Required]
        [Column("HourlyCost")]
        [Display(Name = "Стоимость часа", Description = "Почасовая стоимость")]
        [DataType(DataType.Currency)]
        public decimal HourlyCost { get; set; }

        /// <summary>
        /// Часы в январе
        /// </summary>
        [Column("JanuaryHours")]
        [Display(Name = "Январь", Description = "Плановые часы на январь")]
        [DefaultValue(0)]
        public decimal JanuaryHours { get; set; }

        /// <summary>
        /// Часы в феврале
        /// </summary>
        [Column("FebruaryHours")]
        [Display(Name = "Февраль", Description = "Плановые часы на февраль")]
        [DefaultValue(0)]
        public decimal FebruaryHours { get; set; }

        /// <summary>
        /// Часы в марте
        /// </summary>
        [Column("MarchHours")]
        [Display(Name = "Март", Description = "Плановые часы на март")]
        [DefaultValue(0)]
        public decimal MarchHours { get; set; }

        /// <summary>
        /// Часы в апреле
        /// </summary>
        [Column("AprilHours")]
        [Display(Name = "Апрель", Description = "Плановые часы на апрель")]
        [DefaultValue(0)]
        public decimal AprilHours { get; set; }

        /// <summary>
        /// Часы в мае
        /// </summary>
        [Column("MayHours")]
        [Display(Name = "Май", Description = "Плановые часы на май")]
        [DefaultValue(0)]
        public decimal MayHours { get; set; }

        /// <summary>
        /// Часы в июне
        /// </summary>
        [Column("JuneHours")]
        [Display(Name = "Июнь", Description = "Плановые часы на июнь")]
        [DefaultValue(0)]
        public decimal JuneHours { get; set; }

        /// <summary>
        /// Часы в июле
        /// </summary>
        [Column("JulyHours")]
        [Display(Name = "Июль", Description = "Плановые часы на июль")]
        [DefaultValue(0)]
        public decimal JulyHours { get; set; }

        /// <summary>
        /// Часы в августе
        /// </summary>
        [Column("AugustHours")]
        [Display(Name = "Август", Description = "Плановые часы на август")]
        [DefaultValue(0)]
        public decimal AugustHours { get; set; }

        /// <summary>
        /// Часы в сентябре
        /// </summary>
        [Column("SeptemberHours")]
        [Display(Name = "Сентябрь", Description = "Плановые часы на сентябрь")]
        [DefaultValue(0)]
        public decimal SeptemberHours { get; set; }

        /// <summary>
        /// Часы в октябре
        /// </summary>
        [Column("OctoberHours")]
        [Display(Name = "Октябрь", Description = "Плановые часы на октябрь")]
        [DefaultValue(0)]
        public decimal OctoberHours { get; set; }

        /// <summary>
        /// Часы в ноябре
        /// </summary>
        [Column("NovemberHours")]
        [Display(Name = "Ноябрь", Description = "Плановые часы на ноябрь")]
        [DefaultValue(0)]
        public decimal NovemberHours { get; set; }

        /// <summary>
        /// Часы в декабре
        /// </summary>
        [Column("DecemberHours")]
        [Display(Name = "Декабрь", Description = "Плановые часы на декабрь")]
        [DefaultValue(0)]
        public decimal DecemberHours { get; set; }

        /// <summary>
        /// Всего часов за год (вычисляемое поле)
        /// </summary>
        [Column("TotalYearHours")]
        [Display(Name = "Всего часов", Description = "Суммарное количество часов за год")]
        public decimal TotalYearHours { get; private set; }

        /// <summary>
        /// Общая стоимость за год (вычисляемое поле)
        /// </summary>
        [Column("TotalYearCost")]
        [Display(Name = "Общая стоимость", Description = "Общая стоимость за год")]
        [DataType(DataType.Currency)]
        public decimal TotalYearCost { get; private set; }

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
        /// Техника
        /// </summary>
        [ForeignKey("EquipmentId")]
        public virtual Equipment Equipment { get; set; }

        #endregion

        #region Методы

        /// <summary>
        /// Получить часы по номеру месяца
        /// </summary>
        /// <param name="month">Номер месяца (1-12)</param>
        public decimal GetHoursByMonth(int month)
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
        /// Установить часы по номеру месяца
        /// </summary>
        /// <param name="month">Номер месяца (1-12)</param>
        /// <param name="hours">Количество часов</param>
        public void SetHoursByMonth(int month, decimal hours)
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

        #endregion
    }
}