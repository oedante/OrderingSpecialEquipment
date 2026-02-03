using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория госномеров
    /// </summary>
    public class LicensePlateRepository : GenericRepository<LicensePlate>, ILicensePlateRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public LicensePlateRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает только активные госномера
        /// </summary>
        public async Task<IEnumerable<LicensePlate>> GetActiveLicensePlatesAsync()
        {
            try
            {
                return await _context.LicensePlates
                    .AsNoTracking()
                    .Include(lp => lp.Equipment)
                    .Include(lp => lp.LessorOrganization)
                    .Where(lp => lp.IsActive)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении активных госномеров: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Поиск госномеров по частичному совпадению с защитой от SQL инъекций
        /// </summary>
        public async Task<IEnumerable<LicensePlate>> SearchByPlateNumberAsync(string searchTerm)
        {
            try
            {
                // Экранируем специальные символы для LIKE
                string safeSearch = searchTerm
                    .Replace("[", "[[]")
                    .Replace("%", "[%]")
                    .Replace("_", "[_]");

                return await _context.LicensePlates
                    .AsNoTracking()
                    .Include(lp => lp.Equipment)
                    .Include(lp => lp.LessorOrganization)
                    .Where(lp => EF.Functions.Like(lp.PlateNumber, $"%{safeSearch}%"))
                    .Take(50) // Ограничиваем результаты для производительности
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при поиске госномеров: {ex.Message}");
                throw;
            }
        }
    }
}