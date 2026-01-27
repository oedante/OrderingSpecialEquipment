using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class EquipmentDependencyRepository : GenericRepository<EquipmentDependency>, IEquipmentDependencyRepository
    {
        public EquipmentDependencyRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IList<EquipmentDependency>> GetByMainEquipmentAsync(string mainEquipmentId)
        {
            return await Context.EquipmentDependencies
                                .Where(ed => ed.MainEquipmentId == mainEquipmentId)
                                .OrderBy(ed => ed.DependentEquipmentId)
                                .ToListAsync();
        }

        public async Task<IList<EquipmentDependency>> GetByDependentEquipmentAsync(string dependentEquipmentId)
        {
            return await Context.EquipmentDependencies
                                .Where(ed => ed.DependentEquipmentId == dependentEquipmentId)
                                .OrderBy(ed => ed.MainEquipmentId)
                                .ToListAsync();
        }

        public override async Task<EquipmentDependency?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.EquipmentDependencies
                                    .FirstOrDefaultAsync(ed => ed.Key == intId);
            }
            return null;
        }

        // EquipmentDependency не имеет Id типа string, поэтому метод не переопределяется.
        // public override async Task<EquipmentDependency?> GetByIdStringAsync(string id) => null;
    }
}