using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория пользователей
    /// </summary>
    public interface IUserRepository : IGenericRepository<User>
    {
        /// <summary>
        /// Находит пользователя по Windows логину
        /// </summary>
        Task<User?> GetByWindowsLoginAsync(string windowsLogin);

        /// <summary>
        /// Получает активных пользователей
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<User>> GetActiveUsersAsync();
    }
}