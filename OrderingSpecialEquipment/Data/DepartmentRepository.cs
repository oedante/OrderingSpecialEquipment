using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория отделов
    /// </summary>
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public DepartmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает только активные отделы
        /// </summary>
        public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
        {
            try
            {
                return await _context.Departments
                    .AsNoTracking()
                    .Where(d => d.IsActive)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении активных отделов: {ex.Message}");
                throw;
            }
        }
    }
}