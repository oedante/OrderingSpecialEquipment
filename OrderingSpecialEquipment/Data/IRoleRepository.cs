using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория ролей
    /// </summary>
    public interface IRoleRepository : IGenericRepository<Role>
    {
        /// <summary>
        /// Получает активные роли
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<Role>> GetActiveRolesAsync();
    }
}