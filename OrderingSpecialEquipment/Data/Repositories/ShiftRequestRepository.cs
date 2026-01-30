using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class ShiftRequestRepository : GenericRepository<ShiftRequest>, IShiftRequestRepository
    {
        public ShiftRequestRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IList<ShiftRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await Context.ShiftRequests
                                .Where(sr => sr.Date >= startDate && sr.Date <= endDate)
                                .OrderBy(sr => sr.Date).ThenBy(sr => sr.Shift)
                                .ToListAsync();
        }

        public async Task<IList<ShiftRequest>> GetByDepartmentAsync(string departmentId)
        {
            return await Context.ShiftRequests
                                .Where(sr => sr.DepartmentId == departmentId)
                                .OrderBy(sr => sr.Date).ThenBy(sr => sr.Shift)
                                .ToListAsync();
        }

        public async Task<IList<ShiftRequest>> GetByWarehouseAsync(string warehouseId)
        {
            return await Context.ShiftRequests
                                .Where(sr => sr.WarehouseId == warehouseId)
                                .OrderBy(sr => sr.Date).ThenBy(sr => sr.Shift)
                                .ToListAsync();
        }

        public async Task<IList<ShiftRequest>> GetByEquipmentAsync(string equipmentId)
        {
            return await Context.ShiftRequests
                                .Where(sr => sr.EquipmentId == equipmentId)
                                .OrderBy(sr => sr.Date).ThenBy(sr => sr.Shift)
                                .ToListAsync();
        }

        public async Task<IList<ShiftRequest>> GetByDateAndShiftAsync(DateTime date, int shift)
        {
            return await Context.ShiftRequests
                                .Where(sr => sr.Date == date.Date && sr.Shift == shift)
                                .OrderBy(sr => sr.WarehouseId).ThenBy(sr => sr.EquipmentId)
                                .ToListAsync();
        }

        public async Task<IList<ShiftRequest>> GetByDateRangeAndDepartmentAsync(DateTime startDate, DateTime endDate, string departmentId)
        {
            return await Context.ShiftRequests
                                .Where(sr => sr.Date >= startDate && sr.Date <= endDate && sr.DepartmentId == departmentId)
                                .OrderBy(sr => sr.Date).ThenBy(sr => sr.Shift)
                                .ToListAsync();
        }

        public async Task<IList<ShiftRequest>> GetByDateRangeAndWarehouseAsync(DateTime startDate, DateTime endDate, string warehouseId)
        {
            return await Context.ShiftRequests
                                .Where(sr => sr.Date >= startDate && sr.Date <= endDate && sr.WarehouseId == warehouseId)
                                .OrderBy(sr => sr.Date).ThenBy(sr => sr.Shift)
                                .ToListAsync();
        }

        public async Task<IList<ShiftRequest>> GetUnblockedByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await Context.ShiftRequests
                                .Where(sr => sr.Date >= startDate && sr.Date <= endDate && !sr.IsBlocked)
                                .OrderBy(sr => sr.Date).ThenBy(sr => sr.Shift)
                                .ToListAsync();
        }

        public override async Task<ShiftRequest?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.ShiftRequests
                                    .FirstOrDefaultAsync(sr => sr.Key == intId);
            }
            return null;
        }

        // --- РЕАЛИЗАЦИЯ НОВОГО МЕТОДА ---
        public async Task<IList<ShiftRequest>> FindRequestsAsync(DateTime? date = null, int? shift = null, string? equipmentId = null, string? warehouseId = null, string? departmentId = null)
        {
            var query = Context.ShiftRequests.AsQueryable();

            if (date.HasValue) query = query.Where(r => r.Date.Date == date.Value.Date);
            if (shift.HasValue) query = query.Where(r => r.Shift == shift);
            if (!string.IsNullOrEmpty(equipmentId)) query = query.Where(r => r.EquipmentId == equipmentId);
            if (!string.IsNullOrEmpty(warehouseId)) query = query.Where(r => r.WarehouseId == warehouseId);
            if (!string.IsNullOrEmpty(departmentId)) query = query.Where(r => r.DepartmentId == departmentId);

            return await query.OrderBy(r => r.Date).ThenBy(r => r.Shift).ToListAsync();
        }
    }
}