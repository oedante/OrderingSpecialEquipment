using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface IRoleRepository
    {
        Task<IList<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(object id); // Для поиска по Key (int)
        Task<Role?> GetByIdStringAsync(string id); // Для поиска по Id (string)
        Task<IList<Role>> FindAsync(System.Linq.Expressions.Expression<Func<Role, bool>> predicate);
        Task AddAsync(Role entity);
        void Update(Role entity);
        void Delete(Role entity);
        Task<int> SaveChangesAsync();
        Task<bool> ExistsAsync(string id);
        Task<Role?> GetByCodeAsync(string code);
        Task<IList<Role>> GetActiveAsync();
    }
}