using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsAsync(string id)
        {
            return await Context.Departments.AnyAsync(d => d.Id == id);
        }

        // Переопределяем методы, если нужно добавить специфичную логику
        public override async Task<IList<Department>> GetAllAsync()
        {
            // Например, исключить неактивные по умолчанию
            return await Context.Departments
                                .Where(d => d.IsActive)
                                .OrderBy(d => d.Name)
                                .ToListAsync();
        }

        public override async Task<Department?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.Departments
                                    .FirstOrDefaultAsync(d => d.Key == intId);
            }
            return null;
        }

        public override async Task<Department?> GetByIdStringAsync(string id)
        {
            return await Context.Departments
                                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);
        }
    }
}