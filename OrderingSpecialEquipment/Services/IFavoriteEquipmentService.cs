using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Сервис для управления списком избранной техники пользователя.
    /// </summary>
    public interface IFavoriteEquipmentService
    {
        /// <summary>
        /// Получает список избранной техники для указанного пользователя.
        /// </summary>
        /// <param name="userId">ID пользователя.</param>
        /// <returns>Список моделей Equipment, отсортированный по SortOrder.</returns>
        Task<List<Equipment>> GetFavoriteEquipmentAsync(string userId);

        /// <summary>
        /// Добавляет технику в избранное для пользователя.
        /// </summary>
        /// <param name="userId">ID пользователя.</param>
        /// <param name="equipmentId">ID техники.</param>
        /// <param name="sortOrder">Порядок сортировки.</param>
        /// <returns>True, если успешно добавлено.</returns>
        Task<bool> AddToFavoritesAsync(string userId, string equipmentId, int sortOrder = 0);

        /// <summary>
        /// Удаляет технику из избранного пользователя.
        /// </summary>
        /// <param name="userId">ID пользователя.</param>
        /// <param name="equipmentId">ID техники.</param>
        /// <returns>True, если успешно удалено.</returns>
        Task<bool> RemoveFromFavoritesAsync(string userId, string equipmentId);

        /// <summary>
        /// Проверяет, находится ли техника в избранном у пользователя.
        /// </summary>
        /// <param name="userId">ID пользователя.</param>
        /// <param name="equipmentId">ID техники.</param>
        /// <returns>True, если в избранном.</returns>
        Task<bool> IsFavoriteAsync(string userId, string equipmentId);
    }
}