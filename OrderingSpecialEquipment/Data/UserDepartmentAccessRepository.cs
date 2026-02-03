using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория доступа пользователей к отделам
    /// </summary>
    public class UserDepartmentAccessRepository : GenericRepository<UserDepartmentAccess>, IUserDepartmentAccessRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public UserDepartmentAccessRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает доступы пользователя к отделам
        /// </summary>
        public async Task<IEnumerable<UserDepartmentAccess>> GetUserDepartmentAccessAsync(string userId)
        {
            try
            {
                return await _context.UserDepartmentAccess
                    .AsNoTracking()
                    .Include(uda => uda.User)
                    .Include(uda => uda.Department)
                    .Where(uda => uda.UserId == userId)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении доступа пользователя {userId} к отделам: {ex.Message}");
                throw;
            }
        }
    }
}