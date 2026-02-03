using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория зависимостей техники
    /// </summary>
    public class EquipmentDependencyRepository : GenericRepository<EquipmentDependency>, IEquipmentDependencyRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public EquipmentDependencyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает зависимости для основной техники
        /// </summary>
        public async Task<IEnumerable<EquipmentDependency>> GetDependenciesForMainEquipmentAsync(string mainEquipmentId)
        {
            try
            {
                return await _context.EquipmentDependencies
                    .AsNoTracking()
                    .Include(ed => ed.MainEquipment)
                    .Include(ed => ed.DependentEquipment)
                    .Where(ed => ed.MainEquipmentId == mainEquipmentId)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении зависимостей для техники {mainEquipmentId}: {ex.Message}");
                throw;
            }
        }
    }
}