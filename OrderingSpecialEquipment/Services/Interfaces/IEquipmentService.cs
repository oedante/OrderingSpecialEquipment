using OrderingSpecialEquipment.Models;

namespace OrderingSpecialEquipment.Services.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для работы с техникой
    /// </summary>
    public interface IEquipmentService : IDataService<Equipment>
    {
        /// <summary>
        /// Получение зависимостей для техники
        /// </summary>
        Task<List<EquipmentDependency>> GetDependenciesAsync(string equipmentId);

        /// <summary>
        /// Получение техники с учетом избранного пользователя
        /// </summary>
        Task<List<Equipment>> GetEquipmentsWithFavoritesAsync(string userId, bool onlyFavorites = false);

        /// <summary>
        /// Получение активной техники
        /// </summary>
        Task<List<Equipment>> GetActiveEquipmentsAsync();

        /// <summary>
        /// Добавление в избранное
        /// </summary>
        Task AddToFavoritesAsync(string userId, string equipmentId);

        /// <summary>
        /// Удаление из избранного
        /// </summary>
        Task RemoveFromFavoritesAsync(string userId, string equipmentId);

        /// <summary>
        /// Проверка, находится ли техника в избранном
        /// </summary>
        Task<bool> IsFavoriteAsync(string userId, string equipmentId);
    }
}