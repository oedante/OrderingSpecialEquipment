using System;
using System.ComponentModel.DataAnnotations;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Элемент отчета по транспортной программе
    /// </summary>
    public class TransportProgramReportItem : BaseReportItem
    {
        /// <summary>
        /// Наименование отдела
        /// </summary>
        [Display(Name = "Отдел", Description = "Отдел организации")]
        public string DepartmentName { get; set; } = string.Empty;

        /// <summary>
        /// Наименование техники
        /// </summary>
        [Display(Name = "Техника", Description = "Наименование техники или оборудования")]
        public string EquipmentName { get; set; } = string.Empty;

        /// <summary>
        /// Год транспортной программы
        /// </summary>
        [Display(Name = "Год", Description = "Год транспортной программы")]
        public int Year { get; set; }

        /// <summary>
        /// Стоимость часа работы в рублях (плановая)
        /// </summary>
        [Display(Name = "Ст-ть часа (план)", Description = "Плановая стоимость часа работы в рублях")]
        public decimal HourlyCost { get; set; }

        /// <summary>
        /// Плановые часы за период
        /// </summary>
        [Display(Name = "План (часы)", Description = "Плановые часы работы за период")]
        public decimal PlannedHours { get; set; }

        /// <summary>
        /// Фактически отработанные часы за период
        /// </summary>
        [Display(Name = "Факт (часы)", Description = "Фактически отработанные часы за период")]
        public decimal ActualHours { get; set; }

        /// <summary>
        /// Процент выполнения плана по часам
        /// </summary>
        [Display(Name = "% выполнения", Description = "Процент выполнения плана по часам")]
        public decimal CompletionPercentage => PlannedHours > 0 ? Math.Round((ActualHours / PlannedHours) * 100, 2) : 0;

        /// <summary>
        /// Плановая стоимость за период в рублях
        /// </summary>
        [Display(Name = "План (руб)", Description = "Плановая стоимость работы за период в рублях")]
        public decimal PlannedCost => Math.Round(PlannedHours * HourlyCost, 2);

        /// <summary>
        /// Фактическая стоимость за период в рублях
        /// </summary>
        [Display(Name = "Факт (руб)", Description = "Фактическая стоимость работы за период в рублях")]
        public decimal ActualCost { get; set; }

        /// <summary>
        /// Разница между плановой и фактической стоимостью (экономия/перерасход)
        /// </summary>
        [Display(Name = "Разница", Description = "Разница между плановой и фактической стоимостью (экономия/перерасход)")]
        public decimal CostDifference => Math.Round(ActualCost - PlannedCost, 2);

        /// <summary>
        /// Тип разницы (Экономия или Перерасход)
        /// </summary>
        public string DifferenceType => CostDifference < 0 ? "Экономия" : "Перерасход";

        /// <summary>
        /// Цвет для отображения разницы (зеленый для экономии, красный для перерасхода)
        /// </summary>
        public string DifferenceColor => CostDifference < 0 ? "#2E7D32" : "#C62828";
    }
}