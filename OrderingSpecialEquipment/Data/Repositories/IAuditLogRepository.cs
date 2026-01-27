using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface IAuditLogRepository
    {
        Task<IList<AuditLog>> GetAllAsync();
        Task<AuditLog?> GetByIdAsync(object id); // Для поиска по Key (int)
        // GetByIdStringAsync не нужен, т.к. AuditLog использует Key как первичный ключ (int)
        Task<IList<AuditLog>> FindAsync(System.Linq.Expressions.Expression<Func<AuditLog, bool>> predicate);
        Task AddAsync(AuditLog entity);
        void Update(AuditLog entity);
        void Delete(AuditLog entity);
        Task<int> SaveChangesAsync();
        Task<IList<AuditLog>> GetByTableAsync(string tableName);
        Task<IList<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IList<AuditLog>> GetByUserAsync(string userId);
        Task<IList<AuditLog>> GetByTableAndDateRangeAsync(string tableName, DateTime startDate, DateTime endDate);
    }
}