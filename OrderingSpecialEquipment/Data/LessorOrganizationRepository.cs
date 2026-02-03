using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория организаций-арендодателей
    /// </summary>
    public class LessorOrganizationRepository : GenericRepository<LessorOrganization>, ILessorOrganizationRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public LessorOrganizationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает только активные организации
        /// </summary>
        public async Task<IEnumerable<LessorOrganization>> GetActiveLessorOrganizationsAsync()
        {
            try
            {
                return await _context.LessorOrganizations
                    .AsNoTracking()
                    .Where(lo => lo.IsActive)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении активных организаций-арендодателей: {ex.Message}");
                throw;
            }
        }
    }
}