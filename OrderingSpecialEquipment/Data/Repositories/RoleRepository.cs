using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsAsync(string id)
        {
            return await Context.Roles.AnyAsync(r => r.Id == id); // Используем Id, а не Key
        }

        public async Task<Role?> GetByCodeAsync(string code)
        {
            return await Context.Roles
                                 .FirstOrDefaultAsync(r => r.Code == code);
        }

        public async Task<IList<Role>> GetActiveAsync()
        {
            return await Context.Roles
                                 .Where(r => r.IsActive)
                                 .OrderBy(r => r.Name)
                                 .ToListAsync();
        }

        public override async Task<Role?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.Roles
                                    .FirstOrDefaultAsync(r => r.Key == intId);
            }
            return null;
        }

        public override async Task<Role?> GetByIdStringAsync(string id)
        {
            return await Context.Roles
                                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}