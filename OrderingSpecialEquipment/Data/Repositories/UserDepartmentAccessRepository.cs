using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class UserDepartmentAccessRepository : GenericRepository<UserDepartmentAccess>, IUserDepartmentAccessRepository
    {
        private readonly IUserWarehouseAccessRepository _userWarehouseAccessRepo;

        public UserDepartmentAccessRepository(ApplicationDbContext context, IUserWarehouseAccessRepository userWarehouseAccessRepo) : base(context)
        {
            _userWarehouseAccessRepo = userWarehouseAccessRepo;
        }

        public async Task<IList<UserDepartmentAccess>> GetByUserIdAsync(string userId)
        {
            return await Context.UserDepartmentAccesses
                                .Where(uda => uda.UserId == userId)
                                .ToListAsync();
        }

        public async Task<IList<UserDepartmentAccess>> GetByDepartmentIdAsync(string departmentId)
        {
            return await Context.UserDepartmentAccesses
                                .Where(uda => uda.DepartmentId == departmentId)
                                .ToListAsync();
        }

        public async Task<IList<string>> GetAllowedWarehouseIdsAsync(string userId, string departmentId)
        {
            var userDeptAccesses = await GetByUserIdAsync(userId);
            var relevantAccesses = userDeptAccesses.Where(uda => uda.DepartmentId == departmentId).ToList();

            var allowedWarehouseIds = new List<string>();

            foreach (var access in relevantAccesses)
            {
                if (access.HasAllWarehouses)
                {
                    // Если у доступа к отделу есть HasAllWarehouses, возвращаем null или специальный признак, что все склады разрешены.
                    // В логике авторизации это будет означать "все".
                    return null; // или можно вернуть список всех складов отдела
                }
                else
                {
                    // Иначе получаем список складов, разрешенных для этого конкретного UserDepartmentAccess.Key
                    var specificWarehouseIds = await _userWarehouseAccessRepo.GetWarehouseIdsByUserDeptAccessKeyAsync(access.Key);
                    allowedWarehouseIds.AddRange(specificWarehouseIds);
                }
            }

            return allowedWarehouseIds.Distinct().ToList();
        }

        public override async Task<UserDepartmentAccess?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.UserDepartmentAccesses
                                    .FirstOrDefaultAsync(uda => uda.Key == intId);
            }
            return null;
        }

        // UserDepartmentAccess не имеет Id типа string, поэтому метод не переопределяется.
        // public override async Task<UserDepartmentAccess?> GetByIdStringAsync(string id) => null;
    }
}