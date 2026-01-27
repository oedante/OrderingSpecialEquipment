using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface ITransportProgramRepository
    {
        Task<IList<TransportProgram>> GetAllAsync();
        Task<TransportProgram?> GetByIdAsync(object id); // Для поиска по Key (int)
        // GetByIdStringAsync не нужен, т.к. TransportProgram использует Key как первичный ключ (int)
        Task<IList<TransportProgram>> FindAsync(System.Linq.Expressions.Expression<Func<TransportProgram, bool>> predicate);
        Task AddAsync(TransportProgram entity);
        void Update(TransportProgram entity);
        void Delete(TransportProgram entity);
        Task<int> SaveChangesAsync();
        Task<IList<TransportProgram>> GetByDepartmentAndYearAsync(string departmentId, int year);
        Task<IList<TransportProgram>> GetByEquipmentAndYearAsync(string equipmentId, int year);
        Task<TransportProgram?> GetByDepartmentEquipmentAndYearAsync(string departmentId, string equipmentId, int year);
    }
}