using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface IEquipmentDependencyRepository
    {
        Task<IList<EquipmentDependency>> GetAllAsync();
        Task<EquipmentDependency?> GetByIdAsync(object id); // Для поиска по Key (int)
        // GetByIdStringAsync не нужен, т.к. EquipmentDependency использует Key как первичный ключ (int)
        Task<IList<EquipmentDependency>> FindAsync(System.Linq.Expressions.Expression<Func<EquipmentDependency, bool>> predicate);
        Task AddAsync(EquipmentDependency entity);
        void Update(EquipmentDependency entity);
        void Delete(EquipmentDependency entity);
        Task<int> SaveChangesAsync();
        Task<IList<EquipmentDependency>> GetByMainEquipmentAsync(string mainEquipmentId);
        Task<IList<EquipmentDependency>> GetByDependentEquipmentAsync(string dependentEquipmentId);
    }
}