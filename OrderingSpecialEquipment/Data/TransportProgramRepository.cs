using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория транспортной программы
    /// </summary>
    public class TransportProgramRepository : GenericRepository<TransportProgram>, ITransportProgramRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public TransportProgramRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает программу для отдела за год
        /// </summary>
        public async Task<IEnumerable<TransportProgram>> GetProgramForDepartmentAsync(string departmentId, int year)
        {
            try
            {
                return await _context.TransportProgram
                    .AsNoTracking()
                    .Include(tp => tp.Department)
                    .Include(tp => tp.Equipment)
                    .Where(tp => tp.DepartmentId == departmentId && tp.Year == year)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении транспортной программы для отдела {departmentId} за {year} год: {ex.Message}");
                throw;
            }
        }
    }
}