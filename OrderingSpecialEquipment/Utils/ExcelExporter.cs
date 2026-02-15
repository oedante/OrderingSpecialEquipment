using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;

namespace OrderingSpecialEquipment.Utils
{
    /// <summary>
    /// Класс для экспорта данных в Excel
    /// </summary>
    public static class ExcelExporter
    {
        /// <summary>
        /// Экспорт данных в Excel
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="data">Данные для экспорта</param>
        /// <param name="columns">Колонки (заголовок и функция получения значения)</param>
        /// <param name="sheetName">Имя листа</param>
        /// <param name="fileName">Имя файла (если null, будет предложено сохранить)</param>
        public static void ExportToExcel<T>(
            IEnumerable<T> data,
            List<(string Header, Func<T, object> Value)> columns,
            string sheetName = "Данные",
            string fileName = null)
        {
            try
            {
                // Устанавливаем лицензию EPPlus (для некоммерческого использования)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add(sheetName);

                    // Заголовки
                    for (int i = 0; i < columns.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = columns[i].Header;
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        worksheet.Cells[1, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    // Данные
                    int row = 2;
                    foreach (var item in data)
                    {
                        for (int i = 0; i < columns.Count; i++)
                        {
                            var value = columns[i].Value(item);

                            if (value is DateTime dateTime)
                                worksheet.Cells[row, i + 1].Value = dateTime;
                            else if (value is decimal decimalValue)
                                worksheet.Cells[row, i + 1].Value = decimalValue;
                            else if (value is bool boolValue)
                                worksheet.Cells[row, i + 1].Value = boolValue ? "Да" : "Нет";
                            else
                                worksheet.Cells[row, i + 1].Value = value?.ToString();

                            worksheet.Cells[row, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                        row++;
                    }

                    // Автоширина колонок
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Сохранение файла
                    if (string.IsNullOrEmpty(fileName))
                    {
                        var saveDialog = new Microsoft.Win32.SaveFileDialog
                        {
                            Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                            FileName = $"Экспорт_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                        };

                        if (saveDialog.ShowDialog() == true)
                        {
                            fileName = saveDialog.FileName;
                        }
                        else
                        {
                            return;
                        }
                    }

                    File.WriteAllBytes(fileName, package.GetAsByteArray());

                    MessageBox.Show($"Данные успешно экспортированы в файл:\n{fileName}",
                        "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Открываем файл
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = fileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Экспорт заявок в Excel
        /// </summary>
        public static void ExportShiftRequests(IEnumerable<Models.ShiftRequest> requests)
        {
            var columns = new List<(string, Func<Models.ShiftRequest, object>)>
            {
                ("Дата", r => r.Date),
                ("Смена", r => r.Shift == 0 ? "Дневная" : "Ночная"),
                ("Отдел", r => r.Department?.Name),
                ("Склад", r => r.Warehouse?.Name),
                ("Территория", r => r.Area?.Name),
                ("Техника", r => r.Equipment?.Name),
                ("Госномер", r => r.LicensePlate?.PlateNumber),
                ("Организация", r => r.LessorOrganization?.Name),
                ("Марка", r => r.VehicleBrand),
                ("Кол-во", r => r.RequestedCount),
                ("Часы", r => r.WorkedHours),
                ("Стоимость", r => r.ActualCost),
                ("Отработано", r => r.IsWorked),
                ("Не предоставлена", r => r.IsNotProvided),
                ("Актировка", r => r.IsWeatherCancellation),
                ("Комментарий", r => r.Comment),
                ("Создал", r => r.CreatedByUser?.FullName)
            };

            ExportToExcel(requests, columns, "Заявки");
        }
    }
}