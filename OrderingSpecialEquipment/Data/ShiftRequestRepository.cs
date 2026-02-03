using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация репозитория заявок на смены
    /// </summary>
    public class ShiftRequestRepository : GenericRepository<ShiftRequest>, IShiftRequestRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public ShiftRequestRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Находит заявки по дате и смене
        /// </summary>
        public async Task<IEnumerable<ShiftRequest>> GetByDateAndShiftAsync(DateTime date, int shift)
        {
            try
            {
                return await _context.ShiftRequests
                    .AsNoTracking()
                    .Include(sr => sr.Equipment)
                    .Include(sr => sr.LicensePlate)
                    .ThenInclude(lp => lp!.LessorOrganization)
                    .Include(sr => sr.Warehouse)
                    .ThenInclude(w => w!.Department)
                    .Include(sr => sr.Area)
                    .Include(sr => sr.CreatedByUser)
                    .Include(sr => sr.Department)
                    .Include(sr => sr.LessorOrganization)
                    .Where(sr => sr.Date.Date == date.Date && sr.Shift == shift)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении заявок по дате {date.ToShortDateString()} и смене {shift}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Находит заявки по двум датам и сменам (для отображения ночи предыдущего и дня текущего)
        /// </summary>
        public async Task<IEnumerable<ShiftRequest>> GetByDatesAndShiftsAsync(
            DateTime date1, int shift1,
            DateTime date2, int shift2)
        {
            try
            {
                return await _context.ShiftRequests
                    .AsNoTracking()
                    .Include(sr => sr.Equipment)
                    .Include(sr => sr.LicensePlate)
                    .ThenInclude(lp => lp!.LessorOrganization)
                    .Include(sr => sr.Warehouse)
                    .ThenInclude(w => w!.Department)
                    .Include(sr => sr.Area)
                    .Include(sr => sr.CreatedByUser)
                    .Include(sr => sr.Department)
                    .Include(sr => sr.LessorOrganization)
                    .Where(sr => (sr.Date.Date == date1.Date && sr.Shift == shift1) ||
                                (sr.Date.Date == date2.Date && sr.Shift == shift2))
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении заявок по датам: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получает все заявки с полной информацией
        /// </summary>
        public async Task<IEnumerable<ShiftRequest>> GetAllWithDetailsAsync()
        {
            try
            {
                return await _context.ShiftRequests
                    .AsNoTracking()
                    .Include(sr => sr.Equipment)
                    .Include(sr => sr.LicensePlate)
                    .ThenInclude(lp => lp!.LessorOrganization)
                    .Include(sr => sr.Warehouse)
                    .ThenInclude(w => w!.Department)
                    .Include(sr => sr.Area)
                    .Include(sr => sr.CreatedByUser)
                    .Include(sr => sr.Department)
                    .Include(sr => sr.LessorOrganization)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Ошибка при получении всех заявок: {ex.Message}");
                throw;
            }
        }
    }
}