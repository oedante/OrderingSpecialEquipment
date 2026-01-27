using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface IUserDepartmentAccessRepository
    {
        Task<IList<UserDepartmentAccess>> GetAllAsync();
        Task<UserDepartmentAccess?> GetByIdAsync(object id); // Для поиска по Key (int)
        // GetByIdStringAsync не нужен, т.к. UserDepartmentAccess использует Key как первичный ключ (int)
        Task<IList<UserDepartmentAccess>> FindAsync(System.Linq.Expressions.Expression<Func<UserDepartmentAccess, bool>> predicate);
        Task AddAsync(UserDepartmentAccess entity);
        void Update(UserDepartmentAccess entity);
        void Delete(UserDepartmentAccess entity);
        Task<int> SaveChangesAsync();
        Task<IList<UserDepartmentAccess>> GetByUserIdAsync(string userId);
        Task<IList<UserDepartmentAccess>> GetByDepartmentIdAsync(string departmentId);
        // Специфичный метод для получения связанных складов
        Task<IList<string>> GetAllowedWarehouseIdsAsync(string userId, string departmentId);
    }
}