using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IList<AuditLog>> GetByTableAsync(string tableName)
        {
            return await Context.AuditLogs
                                .Where(al => al.TableName == tableName)
                                .OrderByDescending(al => al.ChangedAt)
                                .ToListAsync();
        }

        public async Task<IList<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await Context.AuditLogs
                                .Where(al => al.ChangedAt >= startDate && al.ChangedAt <= endDate)
                                .OrderByDescending(al => al.ChangedAt)
                                .ToListAsync();
        }

        public async Task<IList<AuditLog>> GetByUserAsync(string userId)
        {
            return await Context.AuditLogs
                                .Where(al => al.ChangedByUserId == userId)
                                .OrderByDescending(al => al.ChangedAt)
                                .ToListAsync();
        }

        public async Task<IList<AuditLog>> GetByTableAndDateRangeAsync(string tableName, DateTime startDate, DateTime endDate)
        {
            return await Context.AuditLogs
                                .Where(al => al.TableName == tableName && al.ChangedAt >= startDate && al.ChangedAt <= endDate)
                                .OrderByDescending(al => al.ChangedAt)
                                .ToListAsync();
        }

        public override async Task<AuditLog?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.AuditLogs
                                    .FirstOrDefaultAsync(al => al.Key == intId);
            }
            return null;
        }

        // AuditLog не имеет Id типа string, поэтому метод не переопределяется.
        // public override async Task<AuditLog?> GetByIdStringAsync(string id) => null;
    }
}