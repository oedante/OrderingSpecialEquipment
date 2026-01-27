using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface IShiftRequestRepository
    {
        Task<IList<ShiftRequest>> GetAllAsync();
        Task<ShiftRequest?> GetByIdAsync(object id); // Для поиска по Key (int)
        // GetByIdStringAsync не нужен, т.к. ShiftRequest использует Key как первичный ключ (int)
        Task<IList<ShiftRequest>> FindAsync(System.Linq.Expressions.Expression<Func<ShiftRequest, bool>> predicate);
        Task AddAsync(ShiftRequest entity);
        void Update(ShiftRequest entity);
        void Delete(ShiftRequest entity);
        Task<int> SaveChangesAsync();
        // Специфичные методы для фильтрации заявок
        Task<IList<ShiftRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IList<ShiftRequest>> GetByDepartmentAsync(string departmentId);
        Task<IList<ShiftRequest>> GetByWarehouseAsync(string warehouseId);
        Task<IList<ShiftRequest>> GetByEquipmentAsync(string equipmentId);
        Task<IList<ShiftRequest>> GetByDateAndShiftAsync(DateTime date, int shift);
        Task<IList<ShiftRequest>> GetByDateRangeAndDepartmentAsync(DateTime startDate, DateTime endDate, string departmentId);
        Task<IList<ShiftRequest>> GetByDateRangeAndWarehouseAsync(DateTime startDate, DateTime endDate, string warehouseId);
        Task<IList<ShiftRequest>> GetUnblockedByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}