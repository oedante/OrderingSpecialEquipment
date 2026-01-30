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

        // --- НОВЫЙ МЕТОД ---
        /// <summary>
        /// Находит заявки по критериям: дата, смена, ID техники, ID склада, ID отдела.
        /// </summary>
        /// <param name="date">Дата (необязательно).</param>
        /// <param name="shift">Смена (необязательно).</param>
        /// <param name="equipmentId">ID техники (необязательно).</param>
        /// <param name="warehouseId">ID склада (необязательно).</param>
        /// <param name="departmentId">ID отдела (необязательно).</param>
        /// <returns>Список найденных заявок.</returns>
        Task<IList<ShiftRequest>> FindRequestsAsync(DateTime? date = null, int? shift = null, string? equipmentId = null, string? warehouseId = null, string? departmentId = null);
    }
}