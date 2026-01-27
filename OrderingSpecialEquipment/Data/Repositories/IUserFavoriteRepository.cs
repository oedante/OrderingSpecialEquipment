using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface IUserFavoriteRepository
    {
        Task<IList<UserFavorite>> GetAllAsync();
        Task<UserFavorite?> GetByIdAsync(object id); // Для поиска по Key (int)
        // GetByIdStringAsync не нужен, т.к. UserFavorite использует Key как первичный ключ (int)
        Task<IList<UserFavorite>> FindAsync(System.Linq.Expressions.Expression<Func<UserFavorite, bool>> predicate);
        Task AddAsync(UserFavorite entity);
        void Update(UserFavorite entity);
        void Delete(UserFavorite entity);
        Task<int> SaveChangesAsync();
        Task<IList<UserFavorite>> GetByUserAsync(string userId);
        Task<IList<UserFavorite>> GetByUserAndEquipmentAsync(string userId, string equipmentId);
        Task<UserFavorite?> GetByUserEquipmentAsync(string userId, string equipmentId);
    }
}