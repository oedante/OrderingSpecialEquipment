using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    // Если у вас есть отдельный интерфейс IEquipmentRepository, он должен наследоваться от IEquipmentRepositoryBase
    // public interface IEquipmentRepository : IEquipmentRepositoryBase
    // {
    //     // Дополнительные методы, если есть
    // }

    // Если IEquipmentRepositoryBase - основной интерфейс:
    public interface IEquipmentRepositoryBase
    {
        Task<IList<Equipment>> GetAllAsync();
        Task<Equipment?> GetByIdAsync(object id); // Для поиска по Key (int)
        Task<Equipment?> GetByIdStringAsync(string id); // Для поиска по Id (string)
        Task<IList<Equipment>> FindAsync(System.Linq.Expressions.Expression<Func<Equipment, bool>> predicate);
        Task AddAsync(Equipment entity);
        void Update(Equipment entity);
        void Delete(Equipment entity);
        Task<int> SaveChangesAsync();
        Task<bool> ExistsAsync(string id);
        // Специфичные методы
        Task<IList<Equipment>> GetActiveAsync();
    }
}