using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Models.Reports;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services.Reports
{
    /// <summary>
    /// Интерфейс сервиса отчетов
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Генерирует отчет по исполнению заявок
        /// </summary>
        Task<(List<ExecutionReportItem> items, ReportSummary summary)> GenerateExecutionReportAsync(ReportParameters parameters);

        /// <summary>
        /// Генерирует отчет по транспортной программе
        /// </summary>
        Task<(List<TransportProgramReportItem> items, ReportSummary summary)> GenerateTransportProgramReportAsync(ReportParameters parameters);

        /// <summary>
        /// Экспортирует отчет в Excel
        /// </summary>
        Task ExportToExcelAsync(object reportData, string reportType, ReportParameters parameters);
    }
}