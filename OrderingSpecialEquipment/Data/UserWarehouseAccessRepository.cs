using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория доступа пользователей к складам
    /// </summary>
    public class UserWarehouseAccessRepository : GenericRepository<UserWarehouseAccess>, IUserWarehouseAccessRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public UserWarehouseAccessRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает доступы пользователя к складам отдела
        /// </summary>
        public async Task<IEnumerable<UserWarehouseAccess>> GetUserWarehouseAccessForDepartmentAsync(int userDepartmentAccessKey)
        {
            try
            {
                return await _context.UserWarehouseAccess
                    .AsNoTracking()
                    .Include(uwa => uwa.UserDepartmentAccess)
                    .Include(uwa => uwa.Warehouse)
                    .Where(uwa => uwa.UserDepartmentAccessKey == userDepartmentAccessKey)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении доступа пользователя к складам отдела {userDepartmentAccessKey}: {ex.Message}");
                throw;
            }
        }
    }
}