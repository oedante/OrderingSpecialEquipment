using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface IWarehouseRepository
    {
        Task<IList<Warehouse>> GetAllAsync();
        Task<Warehouse?> GetByIdAsync(object id); // Для поиска по Key (int)
        Task<Warehouse?> GetByIdStringAsync(string id); // Для поиска по Id (string)
        Task<IList<Warehouse>> FindAsync(System.Linq.Expressions.Expression<Func<Warehouse, bool>> predicate);
        Task AddAsync(Warehouse entity);
        void Update(Warehouse entity);
        void Delete(Warehouse entity);
        Task<int> SaveChangesAsync();
        Task<bool> ExistsAsync(string id);
        Task<IList<Warehouse>> GetByDepartmentAsync(string departmentId);
        Task<IList<Warehouse>> GetActiveByDepartmentAsync(string departmentId);
    }
}