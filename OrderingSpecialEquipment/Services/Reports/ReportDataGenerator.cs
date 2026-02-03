using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services.Reports
{
    /// <summary>
    /// Генератор тестовых данных для отчетов (используется для предпросмотра)
    /// </summary>
    public class ReportDataGenerator
    {
        /// <summary>
        /// Генерирует тестовые данные для отчета по исполнению заявок
        /// </summary>
        public static List<ExecutionReportItem> GenerateExecutionReportTestData(int count = 100)
        {
            var items = new List<ExecutionReportItem>();
            var random = new Random();
            var departments = new[] { "Отдел логистики", "Отдел строительства", "Складской комплекс", "Администрация" };
            var warehouses = new[] { "Склад №1", "Склад №2", "Центральный склад", "Запасной склад" };
            var areas = new[] { "Зона А", "Зона Б", "Зона В", "Открытая площадка", "Крытый ангар" };
            var equipment = new[] { "Автокран 25т", "Автокран 50т", "Автовышка", "Бульдозер", "Экскаватор", "Стропальщик", "Мастер" };
            var organizations = new[] { "ООО 'СтройТех'", "АО 'МехСервис'", "ЗАО 'ТрансМаш'" };
            var brands = new[] { "Liebherr", "XCMG", "Grove", "Caterpillar", "Komatsu" };
            var users = new[] { "Иванов И.И.", "Петров П.П.", "Сидоров С.С.", "Козлов К.К." };

            for (int i = 0; i < count; i++)
            {
                items.Add(new ExecutionReportItem
                {
                    RowNumber = i + 1,
                    Date = DateTime.Today.AddDays(-random.Next(30)),
                    ShiftName = random.Next(2) == 0 ? "Ночная" : "Дневная",
                    DepartmentName = departments[random.Next(departments.Length)],
                    WarehouseName = warehouses[random.Next(warehouses.Length)],
                    AreaName = areas[random.Next(areas.Length)],
                    EquipmentName = equipment[random.Next(equipment.Length)],
                    PlateNumber = $"А{random.Next(100, 999)}{GetRandomLetters(2)}{random.Next(10, 99)}",
                    LessorOrganizationName = organizations[random.Next(organizations.Length)],
                    Brand = brands[random.Next(brands.Length)],
                    RequestedCount = random.Next(1, 5),
                    WorkedHours = random.Next(4, 12),
                    ActualCost = Math.Round(random.Next(5000, 50000) + random.NextDouble(), 2),
                    IsWorked = random.Next(2) == 1,
                    Comment = random.Next(10) > 7 ? "Специальные условия" : "",
                    CreatedByUserName = users[random.Next(users.Length)]
                });
            }

            return items;
        }

        /// <summary>
        /// Генерирует тестовые данные для отчета по транспортной программе
        /// </summary>
        public static List<TransportProgramReportItem> GenerateTransportProgramReportTestData(int count = 50)
        {
            var items = new List<TransportProgramReportItem>();
            var random = new Random();
            var departments = new[] { "Отдел логистики", "Отдел строительства", "Складской комплекс", "Администрация" };
            var equipment = new[] { "Автокран 25т", "Автокран 50т", "Автовышка", "Бульдозер", "Экскаватор", "Стропальщик", "Мастер" };

            for (int i = 0; i < count; i++)
            {
                decimal plannedHours = random.Next(100, 500);
                decimal actualHours = Math.Min(plannedHours, random.Next(50, 600));
                decimal hourlyCost = random.Next(1000, 5000);

                items.Add(new TransportProgramReportItem
                {
                    RowNumber = i + 1,
                    DepartmentName = departments[random.Next(departments.Length)],
                    EquipmentName = equipment[random.Next(equipment.Length)],
                    Year = DateTime.Today.Year,
                    HourlyCost = hourlyCost,
                    PlannedHours = plannedHours,
                    ActualHours = actualHours,
                    ActualCost = actualHours * hourlyCost
                });
            }

            return items;
        }

        /// <summary>
        /// Возвращает случайные буквы для госномера
        /// </summary>
        private static string GetRandomLetters(int count)
        {
            var letters = "АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЩЫЭЮЯ";
            var result = "";
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                result += letters[random.Next(letters.Length)];
            }

            return result;
        }
    }
}