using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория техники
    /// </summary>
    public class EquipmentRepository : GenericRepository<Equipment>, IEquipmentRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public EquipmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает только активную технику
        /// </summary>
        public async Task<IEnumerable<Equipment>> GetActiveEquipmentAsync()
        {
            try
            {
                return await _context.Equipments
                    .AsNoTracking()
                    .Where(e => e.IsActive)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении активной техники: {ex.Message}");
                throw;
            }
        }
    }
}