using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface ILessorOrganizationRepository
    {
        Task<IList<LessorOrganization>> GetAllAsync();
        Task<LessorOrganization?> GetByIdAsync(object id); // Для поиска по Key (int)
        Task<LessorOrganization?> GetByIdStringAsync(string id); // Для поиска по Id (string)
        Task<IList<LessorOrganization>> FindAsync(System.Linq.Expressions.Expression<Func<LessorOrganization, bool>> predicate);
        Task AddAsync(LessorOrganization entity);
        void Update(LessorOrganization entity);
        void Delete(LessorOrganization entity);
        Task<int> SaveChangesAsync();
        Task<bool> ExistsAsync(string id);
        Task<IList<LessorOrganization>> GetActiveAsync();
    }
}