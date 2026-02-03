using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория доступа пользователей к складам
    /// </summary>
    public interface IUserWarehouseAccessRepository : IGenericRepository<UserWarehouseAccess>
    {
        /// <summary>
        /// Получает доступы пользователя к складам отдела
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<UserWarehouseAccess>> GetUserWarehouseAccessForDepartmentAsync(int userDepartmentAccessKey);
    }
}