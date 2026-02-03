using OrderingSpecialEquipment.Models;
using System;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория логов аудита
    /// </summary>
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        /// <summary>
        /// Получает логи за период
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<AuditLog>> GetLogsForPeriodAsync(DateTime startDate, DateTime endDate);
    }
}