using OrderingSpecialEquipment.Models;
using OfficeOpenXml; // EPPlus
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация IExcelExportService с использованием EPPlus.
    /// </summary>
    public class ExcelExportService : IExcelExportService
    {
        static ExcelExportService()
        {
            // Установка лицензии для EPPlus 8+
            // Для некоммерческого использования можно оставить пустым или установить соответствующую лицензию
            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Устарело
        }

        public bool ExportShiftRequestsToExcel(IList<ShiftRequest> requests, string filePath)
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Заявки");

                    var properties = typeof(ShiftRequest).GetProperties();
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var displayAttr = properties[i].GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                        worksheet.Cells[1, i + 1].Value = displayAttr?.Name ?? properties[i].Name;
                    }

                    for (int r = 0; r < requests.Count; r++)
                    {
                        for (int c = 0; c < properties.Length; c++)
                        {
                            var value = properties[c].GetValue(requests[r]);
                            worksheet.Cells[r + 2, c + 1].Value = value?.ToString();
                        }
                    }

                    worksheet.Cells.AutoFitColumns();
                    var fileInfo = new FileInfo(filePath);
                    package.SaveAs(fileInfo); // Используйте SaveAs вместо SaveAs
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка экспорта заявок в Excel: {ex.Message}");
                return false;
            }
        }

        public bool ExportTransportProgramReportToExcel(IList<object> reportData, string filePath)
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Отчет по программе");

                    if (reportData.Any())
                    {
                        var firstItem = reportData.First();
                        var properties = firstItem.GetType().GetProperties();
                        for (int i = 0; i < properties.Length; i++)
                        {
                            var displayAttr = properties[i].GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                            worksheet.Cells[1, i + 1].Value = displayAttr?.Name ?? properties[i].Name;
                        }

                        for (int r = 0; r < reportData.Count; r++)
                        {
                            for (int c = 0; c < properties.Length; c++)
                            {
                                var value = properties[c].GetValue(reportData[r]);
                                worksheet.Cells[r + 2, c + 1].Value = value?.ToString();
                            }
                        }
                    }

                    worksheet.Cells.AutoFitColumns();
                    var fileInfo = new FileInfo(filePath);
                    package.SaveAs(fileInfo); // Используйте SaveAs
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка экспорта отчета в Excel: {ex.Message}");
                return false;
            }
        }

        public bool ExportUsersToExcel(IList<User> users, string filePath)
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Пользователи");

                    var properties = typeof(User).GetProperties();
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var displayAttr = properties[i].GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                        worksheet.Cells[1, i + 1].Value = displayAttr?.Name ?? properties[i].Name;
                    }

                    for (int r = 0; r < users.Count; r++)
                    {
                        for (int c = 0; c < properties.Length; c++)
                        {
                            var value = properties[c].GetValue(users[r]);
                            worksheet.Cells[r + 2, c + 1].Value = value?.ToString();
                        }
                    }

                    worksheet.Cells.AutoFitColumns();
                    var fileInfo = new FileInfo(filePath);
                    package.SaveAs(fileInfo); // Используйте SaveAs
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка экспорта пользователей в Excel: {ex.Message}");
                return false;
            }
        }
    }
}