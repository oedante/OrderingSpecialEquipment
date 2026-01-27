using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface IUserWarehouseAccessRepository
    {
        Task<IList<UserWarehouseAccess>> GetAllAsync();
        Task<UserWarehouseAccess?> GetByIdAsync(object id); // Для поиска по Key (int)
        // GetByIdStringAsync не нужен, т.к. UserWarehouseAccess использует Key как первичный ключ (int)
        Task<IList<UserWarehouseAccess>> FindAsync(System.Linq.Expressions.Expression<Func<UserWarehouseAccess, bool>> predicate);
        Task AddAsync(UserWarehouseAccess entity);
        void Update(UserWarehouseAccess entity);
        void Delete(UserWarehouseAccess entity);
        Task<int> SaveChangesAsync();
        Task<IList<UserWarehouseAccess>> GetByUserDepartmentAccessKeyAsync(int userDepartmentAccessKey);
        Task<IList<UserWarehouseAccess>> GetByUserDepartmentAccessKeysAsync(IEnumerable<int> userDepartmentAccessKeys);
        Task<IList<string>> GetWarehouseIdsByUserDeptAccessKeyAsync(int userDepartmentAccessKey);
    }
}