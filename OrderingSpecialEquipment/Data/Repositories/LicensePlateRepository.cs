using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class LicensePlateRepository : GenericRepository<LicensePlate>, ILicensePlateRepository
    {
        public LicensePlateRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsAsync(string id)
        {
            return await Context.LicensePlates.AnyAsync(l => l.Id == id);
        }

        public async Task<IList<LicensePlate>> GetActiveByEquipmentAsync(string equipmentId)
        {
            return await Context.LicensePlates
                                .Where(l => l.EquipmentId == equipmentId && l.IsActive)
                                .OrderBy(l => l.PlateNumber)
                                .ToListAsync();
        }

        public async Task<IList<LicensePlate>> GetActiveByLessorAsync(string lessorId)
        {
            return await Context.LicensePlates
                                .Where(l => l.LessorOrganizationId == lessorId && l.IsActive)
                                .OrderBy(l => l.PlateNumber)
                                .ToListAsync();
        }

        public override async Task<IList<LicensePlate>> GetAllAsync()
        {
            return await Context.LicensePlates
                                .OrderBy(l => l.PlateNumber)
                                .ToListAsync();
        }

        public override async Task<LicensePlate?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.LicensePlates
                                    .FirstOrDefaultAsync(l => l.Key == intId);
            }
            return null;
        }

        public override async Task<LicensePlate?> GetByIdStringAsync(string id)
        {
            return await Context.LicensePlates
                                .FirstOrDefaultAsync(l => l.Id == id);
        }
    }
}