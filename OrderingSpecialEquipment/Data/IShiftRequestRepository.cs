using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория заявок на смены
    /// </summary>
    public interface IShiftRequestRepository : IGenericRepository<ShiftRequest>
    {
        /// <summary>
        /// Находит заявки по дате и смене
        /// </summary>
        Task<IEnumerable<ShiftRequest>> GetByDateAndShiftAsync(DateTime date, int shift);

        /// <summary>
        /// Находит заявки по датам и сменам
        /// </summary>
        Task<IEnumerable<ShiftRequest>> GetByDatesAndShiftsAsync(
            DateTime date1, int shift1,
            DateTime date2, int shift2);

        /// <summary>
        /// Получает заявки с полной информацией (включая связанные сущности)
        /// </summary>
        Task<IEnumerable<ShiftRequest>> GetAllWithDetailsAsync();
    }
}