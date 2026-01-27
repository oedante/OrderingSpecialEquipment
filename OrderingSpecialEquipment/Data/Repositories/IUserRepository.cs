using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public interface IUserRepository
    {
        Task<IList<User>> GetAllAsync();
        Task<User?> GetByIdAsync(object id); // Для поиска по Key (int)
        Task<User?> GetByIdStringAsync(string id); // Для поиска по Id (string)
        Task<IList<User>> FindAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate);
        Task AddAsync(User entity);
        void Update(User entity);
        void Delete(User entity);
        Task<int> SaveChangesAsync();
        Task<bool> ExistsAsync(string id);
        // --- НОВЫЙ МЕТОД ---
        /// <summary>
        /// Находит пользователя по логину Windows.
        /// </summary>
        /// <param name="windowsLogin">Логин Windows пользователя.</param>
        /// <returns>Модель User или null, если пользователь не найден.</returns>
        Task<User?> GetByWindowsLoginAsync(string windowsLogin);
        // --- /НОВЫЙ МЕТОД ---
        Task<IList<User>> GetActiveAsync();
    }
}