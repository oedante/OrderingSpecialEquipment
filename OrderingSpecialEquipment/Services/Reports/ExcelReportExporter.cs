using OfficeOpenXml;
using OfficeOpenXml.Style;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Models.Reports;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services.Reports
{
    /// <summary>
    /// Экспортер отчетов в Excel с форматированием цен в рублях
    /// </summary>
    public class ExcelReportExporter
    {
        /// <summary>
        /// Экспортирует отчет в Excel файл
        /// </summary>
        public async Task ExportAsync(object reportData, string reportType, ReportParameters parameters)
        {
            // Устанавливаем лицензию EPPlus (бесплатная для некоммерческого использования)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(GetReportTitle(reportType));

            // Настройка стиля заголовка
            var headerStyle = worksheet.Cells[1, 1, 1, GetColumnCount(reportType)];
            headerStyle.Style.Font.Bold = true;
            headerStyle.Style.Font.Size = 12;
            headerStyle.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerStyle.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(230, 230, 230));
            headerStyle.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            headerStyle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerStyle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Настройка стиля итогов
            var summaryStyle = worksheet.Cells[2, 1, 2, GetColumnCount(reportType)];
            summaryStyle.Style.Font.Bold = true;
            summaryStyle.Style.Fill.PatternType = ExcelFillStyle.Solid;
            summaryStyle.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 204));
            summaryStyle.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            // Установка ширины колонок
            worksheet.Column(1).Width = 5;   // №
            worksheet.Column(2).Width = 12;  // Дата/Отдел
            worksheet.Column(3).Width = 10;  // Смена/Техника
            worksheet.Column(4).Width = 15;  // Склад/Год
            worksheet.Column(5).Width = 15;  // Территория/Ст-ть часа
            worksheet.Column(6).Width = 25;  // Техника/План часы
            worksheet.Column(7).Width = 15;  // Госномер/Факт часы
            worksheet.Column(8).Width = 20;  // Организация/% выполнения
            worksheet.Column(9).Width = 15;  // Марка/План руб
            worksheet.Column(10).Width = 12; // Количество/Факт руб
            worksheet.Column(11).Width = 12; // Часы/Разница
            worksheet.Column(12).Width = 15; // Стоимость
            worksheet.Column(13).Width = 12; // Отработала
            worksheet.Column(14).Width = 30; // Комментарий
            worksheet.Column(15).Width = 20; // Пользователь

            // Заполнение данных в зависимости от типа отчета
            if (reportData is (List<ExecutionReportItem> items, ReportSummary summary) executionReport)
            {
                await FillExecutionReport(worksheet, executionReport.items, executionReport.summary, parameters);
            }
            else if (reportData is (List<TransportProgramReportItem> items, ReportSummary summary) transportReport)
            {
                await FillTransportProgramReport(worksheet, transportReport.items, transportReport.summary, parameters);
            }

            // Автоподбор ширины колонок
            worksheet.Cells.AutoFitColumns();

            // Сохранение файла
            var fileName = $"{GetReportTitle(reportType)}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            await package.SaveAsAsync(new FileInfo(filePath));

            // Открытие файла после сохранения
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
        }

        /// <summary>
        /// Заполняет отчет по исполнению заявок
        /// </summary>
        private async Task FillExecutionReport(
            ExcelWorksheet worksheet,
            List<ExecutionReportItem> items,
            ReportSummary summary,
            ReportParameters parameters)
        {
            // Заголовок отчета
            worksheet.Cells[1, 1].Value = "Отчет по исполнению заявок";
            worksheet.Cells[1, 2, 1, 15].Merge = true;

            // Период отчета
            worksheet.Cells[2, 1].Value = $"Период: с {parameters.StartDate:d} по {parameters.EndDate:d}";
            worksheet.Cells[2, 2, 2, 15].Merge = true;

            // Заголовки колонок
            var headerRow = 4;
            worksheet.Cells[headerRow, 1].Value = "№";
            worksheet.Cells[headerRow, 2].Value = "Дата";
            worksheet.Cells[headerRow, 3].Value = "Смена";
            worksheet.Cells[headerRow, 4].Value = "Отдел";
            worksheet.Cells[headerRow, 5].Value = "Склад";
            worksheet.Cells[headerRow, 6].Value = "Территория";
            worksheet.Cells[headerRow, 7].Value = "Техника";
            worksheet.Cells[headerRow, 8].Value = "Госномер";
            worksheet.Cells[headerRow, 9].Value = "Организация";
            worksheet.Cells[headerRow, 10].Value = "Марка";
            worksheet.Cells[headerRow, 11].Value = "Кол-во";
            worksheet.Cells[headerRow, 12].Value = "Часы";
            worksheet.Cells[headerRow, 13].Value = "Стоимость";
            worksheet.Cells[headerRow, 14].Value = "Статус";
            worksheet.Cells[headerRow, 15].Value = "Пользователь";

            // Данные отчета
            var dataStartRow = headerRow + 1;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var row = dataStartRow + i;

                worksheet.Cells[row, 1].Value = item.RowNumber;
                worksheet.Cells[row, 2].Value = item.Date;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "dd.MM.yyyy";
                worksheet.Cells[row, 3].Value = item.ShiftName;
                worksheet.Cells[row, 4].Value = item.DepartmentName;
                worksheet.Cells[row, 5].Value = item.WarehouseName;
                worksheet.Cells[row, 6].Value = item.AreaName;
                worksheet.Cells[row, 7].Value = item.EquipmentName;
                worksheet.Cells[row, 8].Value = item.PlateNumber;
                worksheet.Cells[row, 9].Value = item.LessorOrganizationName;
                worksheet.Cells[row, 10].Value = item.Brand;
                worksheet.Cells[row, 11].Value = item.RequestedCount;
                worksheet.Cells[row, 12].Value = item.WorkedHours;
                worksheet.Cells[row, 12].Style.Numberformat.Format = "0.00";

                // Форматирование стоимости в рублях
                if (item.ActualCost.HasValue)
                {
                    worksheet.Cells[row, 13].Value = item.ActualCost.Value;
                    worksheet.Cells[row, 13].Style.Numberformat.Format = "#,##0.00 ₽";
                }

                worksheet.Cells[row, 14].Value = item.Status;
                worksheet.Cells[row, 14].Style.Font.Color.SetColor(
                    item.IsWorked ?
                    System.Drawing.Color.Green :
                    System.Drawing.Color.Red);

                worksheet.Cells[row, 15].Value = item.CreatedByUserName;
            }

            // Итоговая строка
            var summaryRow = dataStartRow + items.Count + 1;
            worksheet.Cells[summaryRow, 1, summaryRow, 10].Merge = true;
            worksheet.Cells[summaryRow, 1].Value = "ИТОГО:";
            worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;

            worksheet.Cells[summaryRow, 11].Value = summary.TotalRecords;
            worksheet.Cells[summaryRow, 11].Style.Font.Bold = true;

            worksheet.Cells[summaryRow, 12].Value = summary.TotalHours;
            worksheet.Cells[summaryRow, 12].Style.Numberformat.Format = "0.00";
            worksheet.Cells[summaryRow, 12].Style.Font.Bold = true;

            worksheet.Cells[summaryRow, 13].Value = summary.TotalAmount;
            worksheet.Cells[summaryRow, 13].Style.Numberformat.Format = "#,##0.00 ₽";
            worksheet.Cells[summaryRow, 13].Style.Font.Bold = true;

            worksheet.Cells[summaryRow, 14].Value = $"{summary.CompletionPercentage:F2}%";
            worksheet.Cells[summaryRow, 14].Style.Font.Bold = true;
        }

        /// <summary>
        /// Заполняет отчет по транспортной программе
        /// </summary>
        private async Task FillTransportProgramReport(
            ExcelWorksheet worksheet,
            List<TransportProgramReportItem> items,
            ReportSummary summary,
            ReportParameters parameters)
        {
            // Заголовок отчета
            worksheet.Cells[1, 1].Value = "Отчет по транспортной программе";
            worksheet.Cells[1, 2, 1, 11].Merge = true;

            // Период отчета
            string periodText = parameters.Month.HasValue
                ? $"Год: {parameters.Year}, Месяц: {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(parameters.Month.Value)}"
                : $"Год: {parameters.Year}";

            worksheet.Cells[2, 1].Value = periodText;
            worksheet.Cells[2, 2, 2, 11].Merge = true;

            // Заголовки колонок
            var headerRow = 4;
            worksheet.Cells[headerRow, 1].Value = "№";
            worksheet.Cells[headerRow, 2].Value = "Отдел";
            worksheet.Cells[headerRow, 3].Value = "Техника";
            worksheet.Cells[headerRow, 4].Value = "Год";
            worksheet.Cells[headerRow, 5].Value = "Ст-ть часа";
            worksheet.Cells[headerRow, 6].Value = "План (часы)";
            worksheet.Cells[headerRow, 7].Value = "Факт (часы)";
            worksheet.Cells[headerRow, 8].Value = "% выполнения";
            worksheet.Cells[headerRow, 9].Value = "План (руб)";
            worksheet.Cells[headerRow, 10].Value = "Факт (руб)";
            worksheet.Cells[headerRow, 11].Value = "Разница";

            // Данные отчета
            var dataStartRow = headerRow + 1;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var row = dataStartRow + i;

                worksheet.Cells[row, 1].Value = item.RowNumber;
                worksheet.Cells[row, 2].Value = item.DepartmentName;
                worksheet.Cells[row, 3].Value = item.EquipmentName;
                worksheet.Cells[row, 4].Value = item.Year;
                worksheet.Cells[row, 5].Value = item.HourlyCost;
                worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00 ₽";

                worksheet.Cells[row, 6].Value = item.PlannedHours;
                worksheet.Cells[row, 6].Style.Numberformat.Format = "0.00";

                worksheet.Cells[row, 7].Value = item.ActualHours;
                worksheet.Cells[row, 7].Style.Numberformat.Format = "0.00";

                worksheet.Cells[row, 8].Value = item.CompletionPercentage;
                worksheet.Cells[row, 8].Style.Numberformat.Format = "0.00%";
                worksheet.Cells[row, 8].Style.Font.Color.SetColor(
                    item.CompletionPercentage >= 100 ?
                    System.Drawing.Color.Green :
                    System.Drawing.Color.Orange);

                worksheet.Cells[row, 9].Value = item.PlannedCost;
                worksheet.Cells[row, 9].Style.Numberformat.Format = "#,##0.00 ₽";

                worksheet.Cells[row, 10].Value = item.ActualCost;
                worksheet.Cells[row, 10].Style.Numberformat.Format = "#,##0.00 ₽";

                // Разница с цветовым выделением
                worksheet.Cells[row, 11].Value = item.CostDifference;
                worksheet.Cells[row, 11].Style.Numberformat.Format = "#,##0.00 ₽";
                worksheet.Cells[row, 11].Style.Font.Color.SetColor(
                    System.Drawing.ColorTranslator.FromHtml(item.DifferenceColor));
                worksheet.Cells[row, 11].Style.Font.Bold = true;
            }

            // Итоговая строка
            var summaryRow = dataStartRow + items.Count + 1;
            worksheet.Cells[summaryRow, 1, summaryRow, 4].Merge = true;
            worksheet.Cells[summaryRow, 1].Value = "ИТОГО:";
            worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;

            worksheet.Cells[summaryRow, 5].Value = summary.AverageHourlyCost;
            worksheet.Cells[summaryRow, 5].Style.Numberformat.Format = "#,##0.00 ₽";
            worksheet.Cells[summaryRow, 5].Style.Font.Bold = true;

            worksheet.Cells[summaryRow, 6].Value = summary.TotalHours;
            worksheet.Cells[summaryRow, 6].Style.Numberformat.Format = "0.00";
            worksheet.Cells[summaryRow, 6].Style.Font.Bold = true;

            worksheet.Cells[summaryRow, 7].Value = items.Sum(i => i.ActualHours);
            worksheet.Cells[summaryRow, 7].Style.Numberformat.Format = "0.00";
            worksheet.Cells[summaryRow, 7].Style.Font.Bold = true;

            worksheet.Cells[summaryRow, 8].Value = summary.CompletionPercentage;
            worksheet.Cells[summaryRow, 8].Style.Numberformat.Format = "0.00%";
            worksheet.Cells[summaryRow, 8].Style.Font.Bold = true;

            worksheet.Cells[summaryRow, 9].Value = summary.TotalAmount;
            worksheet.Cells[summaryRow, 9].Style.Numberformat.Format = "#,##0.00 ₽";
            worksheet.Cells[summaryRow, 9].Style.Font.Bold = true;

            worksheet.Cells[summaryRow, 10].Value = items.Sum(i => i.ActualCost);
            worksheet.Cells[summaryRow, 10].Style.Numberformat.Format = "#,##0.00 ₽";
            worksheet.Cells[summaryRow, 10].Style.Font.Bold = true;

            worksheet.Cells[summaryRow, 11].Value = items.Sum(i => i.CostDifference);
            worksheet.Cells[summaryRow, 11].Style.Numberformat.Format = "#,##0.00 ₽";
            worksheet.Cells[summaryRow, 11].Style.Font.Bold = true;
            worksheet.Cells[summaryRow, 11].Style.Font.Color.SetColor(
                items.Sum(i => i.CostDifference) < 0 ?
                System.Drawing.Color.Green :
                System.Drawing.Color.Red);
        }

        /// <summary>
        /// Возвращает заголовок отчета по типу
        /// </summary>
        private string GetReportTitle(string reportType) => reportType switch
        {
            "Execution" => "Отчет_исполнение",
            "TransportProgram" => "Отчет_транспортная_программа",
            _ => "Отчет"
        };

        /// <summary>
        /// Возвращает количество колонок по типу отчета
        /// </summary>
        private int GetColumnCount(string reportType) => reportType switch
        {
            "Execution" => 15,
            "TransportProgram" => 11,
            _ => 10
        };
    }
}