using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class UserWarehouseAccessRepository : GenericRepository<UserWarehouseAccess>, IUserWarehouseAccessRepository
    {
        public UserWarehouseAccessRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IList<UserWarehouseAccess>> GetByUserDepartmentAccessKeyAsync(int userDepartmentAccessKey)
        {
            return await Context.UserWarehouseAccesses
                                .Where(uwa => uwa.UserDepartmentAccessKey == userDepartmentAccessKey)
                                .ToListAsync();
        }

        public async Task<IList<UserWarehouseAccess>> GetByUserDepartmentAccessKeysAsync(IEnumerable<int> userDepartmentAccessKeys)
        {
            return await Context.UserWarehouseAccesses
                                .Where(uwa => userDepartmentAccessKeys.Contains(uwa.UserDepartmentAccessKey))
                                .ToListAsync();
        }

        public async Task<IList<string>> GetWarehouseIdsByUserDeptAccessKeyAsync(int userDepartmentAccessKey)
        {
            return await Context.UserWarehouseAccesses
                                .Where(uwa => uwa.UserDepartmentAccessKey == userDepartmentAccessKey)
                                .Select(uwa => uwa.WarehouseId)
                                .ToListAsync();
        }

        public override async Task<UserWarehouseAccess?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.UserWarehouseAccesses
                                    .FirstOrDefaultAsync(uwa => uwa.Key == intId);
            }
            return null;
        }

        // UserWarehouseAccess не имеет Id типа string, поэтому метод не переопределяется.
        // public override async Task<UserWarehouseAccess?> GetByIdStringAsync(string id) => null;
    }
}