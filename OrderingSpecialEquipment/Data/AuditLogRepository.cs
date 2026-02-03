using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория логов аудита
    /// </summary>
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает логи за период
        /// </summary>
        public async Task<IEnumerable<AuditLog>> GetLogsForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.AuditLogs
                    .AsNoTracking()
                    .Include(al => al.ChangedByUser)
                    .Where(al => al.ChangedAt >= startDate && al.ChangedAt <= endDate)
                    .OrderByDescending(al => al.ChangedAt)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении логов за период {startDate} - {endDate}: {ex.Message}");
                throw;
            }
        }
    }
}