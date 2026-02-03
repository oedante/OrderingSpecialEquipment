using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Интерфейс сервиса избранной техники
    /// Управляет добавлением/удалением техники в избранное для пользователя
    /// </summary>
    public interface IFavoriteEquipmentService
    {
        /// <summary>
        /// Получает избранную технику для пользователя
        /// </summary>
        Task<IEnumerable<Equipment>> GetFavoriteEquipmentAsync(string userId);

        /// <summary>
        /// Добавляет технику в избранное
        /// </summary>
        Task AddToFavoriteAsync(string userId, string equipmentId);

        /// <summary>
        /// Удаляет технику из избранного
        /// </summary>
        Task RemoveFromFavoriteAsync(string userId, string equipmentId);

        /// <summary>
        /// Проверяет, находится ли техника в избранном
        /// </summary>
        Task<bool> IsFavoriteAsync(string userId, string equipmentId);
    }
}