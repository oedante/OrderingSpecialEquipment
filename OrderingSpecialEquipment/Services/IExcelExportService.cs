using OrderingSpecialEquipment.Models;
using System.Collections.Generic;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Интерфейс для сервиса экспорта данных в Excel.
    /// </summary>
    public interface IExcelExportService
    {
        /// <summary>
        /// Экспортирует список заявок (ShiftRequests) в файл Excel.
        /// </summary>
        /// <param name="requests">Список заявок для экспорта.</param>
        /// <param name="filePath">Путь к файлу Excel для сохранения.</param>
        /// <returns>True, если экспорт успешен, иначе false.</returns>
        bool ExportShiftRequestsToExcel(IList<ShiftRequest> requests, string filePath);

        /// <summary>
        /// Экспортирует сводный отчет по транспортной программе в файл Excel.
        /// </summary>
        /// <param name="reportData">Данные отчета (например, список объектов с полями для отчета).</param>
        /// <param name="filePath">Путь к файлу Excel для сохранения.</param>
        /// <returns>True, если экспорт успешен, иначе false.</returns>
        bool ExportTransportProgramReportToExcel(IList<object> reportData, string filePath);

        /// <summary>
        /// Экспортирует список пользователей в файл Excel.
        /// </summary>
        /// <param name="users">Список пользователей для экспорта.</param>
        /// <param name="filePath">Путь к файлу Excel для сохранения.</param>
        /// <returns>True, если экспорт успешен, иначе false.</returns>
        bool ExportUsersToExcel(IList<User> users, string filePath);

        // ... другие методы экспорта для других сущностей ...
    }
}