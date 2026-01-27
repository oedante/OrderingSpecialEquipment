using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы транспортной программы.
    /// </summary>
    [Table("TransportProgram")]
    public class TransportProgram
    {
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Уникальный ключ записи программы")]
        public int Key { get; set; } // SERIAL

        [Column("DepartmentId")]
        [StringLength(10)]
        [Display(Name = "ID отдела", Description = "ID отдела, для которого формируется программа")]
        public string DepartmentId { get; set; } = string.Empty;

        [Column("Year")]
        [Display(Name = "Год", Description = "Год, на который составляется программа")]
        public int Year { get; set; }

        [Column("EquipmentId")]
        [StringLength(10)]
        [Display(Name = "ID техники", Description = "ID техники, включенной в программу")]
        public string EquipmentId { get; set; } = string.Empty;

        [Column("HourlyCost")]
        [Display(Name = "Стоимость часа", Description = "Стоимость часа работы в рублях (для расчетов в программе)")]
        public decimal HourlyCost { get; set; }

        [Column("JanuaryHours")]
        [Display(Name = "Часы в январе", Description = "Предусмотренные часы работы в январе")]
        public decimal JanuaryHours { get; set; } = 0;

        [Column("FebruaryHours")]
        [Display(Name = "Часы в феврале", Description = "Предусмотренные часы работы в феврале")]
        public decimal FebruaryHours { get; set; } = 0;

        [Column("MarchHours")]
        [Display(Name = "Часы в марте", Description = "Предусмотренные часы работы в марте")]
        public decimal MarchHours { get; set; } = 0;

        [Column("AprilHours")]
        [Display(Name = "Часы в апреле", Description = "Предусмотренные часы работы в апреле")]
        public decimal AprilHours { get; set; } = 0;

        [Column("MayHours")]
        [Display(Name = "Часы в мае", Description = "Предусмотренные часы работы в мае")]
        public decimal MayHours { get; set; } = 0;

        [Column("JuneHours")]
        [Display(Name = "Часы в июне", Description = "Предусмотренные часы работы в июне")]
        public decimal JuneHours { get; set; } = 0;

        [Column("JulyHours")]
        [Display(Name = "Часы в июле", Description = "Предусмотренные часы работы в июле")]
        public decimal JulyHours { get; set; } = 0;

        [Column("AugustHours")]
        [Display(Name = "Часы в августе", Description = "Предусмотренные часы работы в августе")]
        public decimal AugustHours { get; set; } = 0;

        [Column("SeptemberHours")]
        [Display(Name = "Часы в сентябре", Description = "Предусмотренные часы работы в сентябре")]
        public decimal SeptemberHours { get; set; } = 0;

        [Column("OctoberHours")]
        [Display(Name = "Часы в октябре", Description = "Предусмотренные часы работы в октябре")]
        public decimal OctoberHours { get; set; } = 0;

        [Column("NovemberHours")]
        [Display(Name = "Часы в ноябре", Description = "Предусмотренные часы работы в ноябре")]
        public decimal NovemberHours { get; set; } = 0;

        [Column("DecemberHours")]
        [Display(Name = "Часы в декабре", Description = "Предусмотренные часы работы в декабре")]
        public decimal DecemberHours { get; set; } = 0;

        // Эти столбцы генерируются в БД, но можно добавить как вычисляемые свойства в C#
        [NotMapped] // Указывает EF, что это свойство не отображается на столбец
        [Display(Name = "Всего часов за год", Description = "Суммарные предусмотренные часы за год (вычисляемое)")]
        public decimal TotalYearHours => JanuaryHours + FebruaryHours + MarchHours + AprilHours + MayHours + JuneHours + JulyHours + AugustHours + SeptemberHours + OctoberHours + NovemberHours + DecemberHours;

        [NotMapped]
        [Display(Name = "Всего стоимость за год", Description = "Суммарная предусмотренная стоимость за год (вычисляемое)")]
        public decimal TotalYearCost => TotalYearHours * HourlyCost;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства (опционально, для EF)
        public virtual Department Department { get; set; } = null!;
        public virtual Equipment Equipment { get; set; } = null!;
    }
}