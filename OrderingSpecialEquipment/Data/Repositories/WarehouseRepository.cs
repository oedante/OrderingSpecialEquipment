using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class WarehouseRepository : GenericRepository<Warehouse>, IWarehouseRepository
    {
        public WarehouseRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsAsync(string id)
        {
            return await Context.Warehouses.AnyAsync(w => w.Id == id);
        }

        public async Task<IList<Warehouse>> GetByDepartmentAsync(string departmentId)
        {
            return await Context.Warehouses
                                .Where(w => w.DepartmentId == departmentId)
                                .OrderBy(w => w.Name)
                                .ToListAsync();
        }

        public async Task<IList<Warehouse>> GetActiveByDepartmentAsync(string departmentId)
        {
            return await Context.Warehouses
                                .Where(w => w.DepartmentId == departmentId && w.IsActive)
                                .OrderBy(w => w.Name)
                                .ToListAsync();
        }

        public override async Task<IList<Warehouse>> GetAllAsync()
        {
            return await Context.Warehouses
                                .OrderBy(w => w.Name)
                                .ToListAsync();
        }

        public override async Task<Warehouse?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.Warehouses
                                    .FirstOrDefaultAsync(w => w.Key == intId);
            }
            return null;
        }

        public override async Task<Warehouse?> GetByIdStringAsync(string id)
        {
            return await Context.Warehouses
                                .FirstOrDefaultAsync(w => w.Id == id);
        }
    }
}