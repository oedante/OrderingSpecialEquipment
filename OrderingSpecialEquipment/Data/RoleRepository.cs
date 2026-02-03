using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория ролей
    /// </summary>
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public RoleRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает только активные роли
        /// </summary>
        public async Task<IEnumerable<Role>> GetActiveRolesAsync()
        {
            try
            {
                return await _context.Roles
                    .AsNoTracking()
                    .Where(r => r.IsActive)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении активных ролей: {ex.Message}");
                throw;
            }
        }
    }
}