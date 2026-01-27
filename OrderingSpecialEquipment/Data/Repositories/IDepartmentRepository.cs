using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface IDepartmentRepository
    {
        Task<IList<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(object id); // Для поиска по Key (int)
        Task<Department?> GetByIdStringAsync(string id); // Для поиска по Id (string)
        Task<IList<Department>> FindAsync(System.Linq.Expressions.Expression<Func<Department, bool>> predicate);
        Task AddAsync(Department entity);
        void Update(Department entity);
        void Delete(Department entity);
        Task<int> SaveChangesAsync();
        // Специфичные методы для Department
        Task<bool> ExistsAsync(string id);
    }
}