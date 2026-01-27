using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    // Убедитесь, что имя интерфейса здесь совпадает с именем в IEquipmentRepositoryBase.cs
    public class EquipmentRepository : GenericRepository<Equipment>, IEquipmentRepositoryBase // <- Это имя должно совпадать с интерфейсом
    {
        public EquipmentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsAsync(string id)
        {
            return await Context.Equipments.AnyAsync(e => e.Id == id);
        }

        public async Task<IList<Equipment>> GetActiveAsync()
        {
            return await Context.Equipments
                                .Where(e => e.IsActive)
                                .OrderBy(e => e.Name)
                                .ToListAsync();
        }

        public override async Task<IList<Equipment>> GetAllAsync()
        {
            return await Context.Equipments
                                .OrderBy(e => e.Name)
                                .ToListAsync();
        }

        public override async Task<Equipment?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.Equipments
                                    .FirstOrDefaultAsync(e => e.Key == intId);
            }
            return null;
        }

        public override async Task<Equipment?> GetByIdStringAsync(string id)
        {
            return await Context.Equipments
                                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}