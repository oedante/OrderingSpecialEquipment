using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class LessorOrganizationRepository : GenericRepository<LessorOrganization>, ILessorOrganizationRepository
    {
        public LessorOrganizationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsAsync(string id)
        {
            return await Context.LessorOrganizations.AnyAsync(l => l.Id == id);
        }

        public async Task<IList<LessorOrganization>> GetActiveAsync()
        {
            return await Context.LessorOrganizations
                                .Where(l => l.IsActive)
                                .OrderBy(l => l.Name)
                                .ToListAsync();
        }

        public override async Task<IList<LessorOrganization>> GetAllAsync()
        {
            return await Context.LessorOrganizations
                                .OrderBy(l => l.Name)
                                .ToListAsync();
        }

        public override async Task<LessorOrganization?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.LessorOrganizations
                                    .FirstOrDefaultAsync(l => l.Key == intId);
            }
            return null;
        }

        public override async Task<LessorOrganization?> GetByIdStringAsync(string id)
        {
            return await Context.LessorOrganizations
                                .FirstOrDefaultAsync(l => l.Id == id);
        }
    }
}