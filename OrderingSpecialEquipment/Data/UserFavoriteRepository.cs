using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория избранного пользователей
    /// </summary>
    public class UserFavoriteRepository : GenericRepository<UserFavorite>, IUserFavoriteRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public UserFavoriteRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает избранную технику пользователя
        /// </summary>
        public async Task<IEnumerable<UserFavorite>> GetUserFavoritesAsync(string userId)
        {
            try
            {
                return await _context.UserFavorites
                    .AsNoTracking()
                    .Include(uf => uf.User)
                    .Include(uf => uf.Equipment)
                    .Where(uf => uf.UserId == userId)
                    .OrderBy(uf => uf.SortOrder)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении избранной техники пользователя {userId}: {ex.Message}");
                throw;
            }
        }
    }
}