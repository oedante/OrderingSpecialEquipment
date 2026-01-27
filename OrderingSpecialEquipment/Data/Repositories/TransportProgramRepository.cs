using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class TransportProgramRepository : GenericRepository<TransportProgram>, ITransportProgramRepository
    {
        public TransportProgramRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IList<TransportProgram>> GetByDepartmentAndYearAsync(string departmentId, int year)
        {
            return await Context.TransportPrograms
                                 .Where(tp => tp.DepartmentId == departmentId && tp.Year == year)
                                 .OrderBy(tp => tp.EquipmentId)
                                 .ToListAsync();
        }

        public async Task<IList<TransportProgram>> GetByEquipmentAndYearAsync(string equipmentId, int year)
        {
            return await Context.TransportPrograms
                                 .Where(tp => tp.EquipmentId == equipmentId && tp.Year == year)
                                 .OrderBy(tp => tp.DepartmentId)
                                 .ToListAsync();
        }

        public async Task<TransportProgram?> GetByDepartmentEquipmentAndYearAsync(string departmentId, string equipmentId, int year)
        {
            return await Context.TransportPrograms
                                 .FirstOrDefaultAsync(tp => tp.DepartmentId == departmentId && tp.EquipmentId == equipmentId && tp.Year == year);
        }

        public override async Task<TransportProgram?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.TransportPrograms
                                    .FirstOrDefaultAsync(tp => tp.Key == intId);
            }
            return null;
        }

        // TransportProgram не имеет Id типа string, поэтому метод не переопределяется.
        // public override async Task<TransportProgram?> GetByIdStringAsync(string id) => null;
    }
}