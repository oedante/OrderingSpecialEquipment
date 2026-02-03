using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория доступа пользователей к отделам
    /// </summary>
    public interface IUserDepartmentAccessRepository : IGenericRepository<UserDepartmentAccess>
    {
        /// <summary>
        /// Получает доступы пользователя к отделам
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<UserDepartmentAccess>> GetUserDepartmentAccessAsync(string userId);
    }
}