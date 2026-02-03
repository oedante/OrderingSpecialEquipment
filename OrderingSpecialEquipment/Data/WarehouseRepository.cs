using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория складов
    /// </summary>
    public class WarehouseRepository : GenericRepository<Warehouse>, IWarehouseRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public WarehouseRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает только активные склады
        /// </summary>
        public async Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync()
        {
            try
            {
                return await _context.Warehouses
                    .AsNoTracking()
                    .Include(w => w.Department)
                    .Where(w => w.IsActive)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении активных складов: {ex.Message}");
                throw;
            }
        }
    }
}