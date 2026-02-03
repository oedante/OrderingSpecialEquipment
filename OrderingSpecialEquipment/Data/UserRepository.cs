using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория пользователей
    /// </summary>
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public UserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Находит пользователя по Windows логину
        /// </summary>
        public async Task<User?> GetByWindowsLoginAsync(string windowsLogin)
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .Include(u => u.Role)
                    .Include(u => u.DefaultDepartment)
                    .FirstOrDefaultAsync(u => u.WindowsLogin == windowsLogin && u.IsActive);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при поиске пользователя по логину {windowsLogin}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получает только активных пользователей
        /// </summary>
        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .Include(u => u.Role)
                    .Include(u => u.DefaultDepartment)
                    .Where(u => u.IsActive)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении активных пользователей: {ex.Message}");
                throw;
            }
        }
    }
}