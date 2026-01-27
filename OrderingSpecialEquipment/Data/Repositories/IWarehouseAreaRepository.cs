using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface IWarehouseAreaRepository
    {
        Task<IList<WarehouseArea>> GetAllAsync();
        Task<WarehouseArea?> GetByIdAsync(object id); // Для поиска по Key (int)
        Task<WarehouseArea?> GetByIdStringAsync(string id); // Для поиска по Id (string)
        Task<IList<WarehouseArea>> FindAsync(System.Linq.Expressions.Expression<Func<WarehouseArea, bool>> predicate);
        Task AddAsync(WarehouseArea entity);
        void Update(WarehouseArea entity);
        void Delete(WarehouseArea entity);
        Task<int> SaveChangesAsync();
        Task<bool> ExistsAsync(string id);
        Task<IList<WarehouseArea>> GetByWarehouseAsync(string warehouseId);
        Task<IList<WarehouseArea>> GetActiveByWarehouseAsync(string warehouseId);
    }
}