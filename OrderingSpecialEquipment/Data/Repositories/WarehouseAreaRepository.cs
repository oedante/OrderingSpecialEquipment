using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class WarehouseAreaRepository : GenericRepository<WarehouseArea>, IWarehouseAreaRepository
    {
        public WarehouseAreaRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsAsync(string id)
        {
            return await Context.WarehouseAreas.AnyAsync(wa => wa.Id == id);
        }

        public async Task<IList<WarehouseArea>> GetByWarehouseAsync(string warehouseId)
        {
            return await Context.WarehouseAreas
                                .Where(wa => wa.WarehouseId == warehouseId)
                                .OrderBy(wa => wa.Name)
                                .ToListAsync();
        }

        public async Task<IList<WarehouseArea>> GetActiveByWarehouseAsync(string warehouseId)
        {
            return await Context.WarehouseAreas
                                .Where(wa => wa.WarehouseId == warehouseId && wa.IsActive)
                                .OrderBy(wa => wa.Name)
                                .ToListAsync();
        }

        public override async Task<IList<WarehouseArea>> GetAllAsync()
        {
            return await Context.WarehouseAreas
                                .OrderBy(wa => wa.Name)
                                .ToListAsync();
        }

        public override async Task<WarehouseArea?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.WarehouseAreas
                                    .FirstOrDefaultAsync(wa => wa.Key == intId);
            }
            return null;
        }

        public override async Task<WarehouseArea?> GetByIdStringAsync(string id)
        {
            return await Context.WarehouseAreas
                                .FirstOrDefaultAsync(wa => wa.Id == id);
        }
    }
}