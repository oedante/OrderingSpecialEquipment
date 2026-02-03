using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория избранного пользователей
    /// </summary>
    public interface IUserFavoriteRepository : IGenericRepository<UserFavorite>
    {
        /// <summary>
        /// Получает избранную технику пользователя
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<UserFavorite>> GetUserFavoritesAsync(string userId);
    }
}