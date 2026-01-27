using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface ILicensePlateRepository
    {
        Task<IList<LicensePlate>> GetAllAsync();
        Task<LicensePlate?> GetByIdAsync(object id); // Для поиска по Key (int)
        Task<LicensePlate?> GetByIdStringAsync(string id); // Для поиска по Id (string)
        Task<IList<LicensePlate>> FindAsync(System.Linq.Expressions.Expression<Func<LicensePlate, bool>> predicate);
        Task AddAsync(LicensePlate entity);
        void Update(LicensePlate entity);
        void Delete(LicensePlate entity);
        Task<int> SaveChangesAsync();
        Task<bool> ExistsAsync(string id);
        Task<IList<LicensePlate>> GetActiveByEquipmentAsync(string equipmentId);
        Task<IList<LicensePlate>> GetActiveByLessorAsync(string lessorId);
    }
}